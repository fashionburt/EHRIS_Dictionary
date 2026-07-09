window.DIC1999R02 = {
    dt: null,
    urls: null,

    // HTML 轉碼輔助，防止 XSS 與顯示錯誤
    htmlEncode: function (value) {
        if (!value) return '';
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    },

    init: function () {
        console.log("DIC1999R02 初始化開始");
        const self = this;
        self.urls = window.permissionAction;
        const tableId = '#tableColumns';

        if ($(tableId).length === 0) return;

        // 清理舊有的 DataTable 實例
        if ($.fn.DataTable.isDataTable(tableId)) {
            $(tableId).DataTable().destroy();
        }

        // 初始化欄位清單 DataTable
        self.dt = $(tableId).DataTable({
            serverSide: true,
            processing: true,
            searching: false,
            ordering: false,
            ajax: {
                url: `${self.urls.getData}?sid=${self.urls.sid}`,
                type: 'POST',
                contentType: 'application/json',
                data: function (d) {
                    return JSON.stringify({
                        draw: d.draw,
                        start: d.start,
                        length: d.length,
                        dbKey: self.urls.dbKey,
                        tableName: self.urls.tableName,
                        sid: self.urls.sid,
                        extraSearch: {
                            searchValue: $('#columnSearchKeyword').val()
                        }
                    });
                }
            },
            columns: [
                { data: 'rowName', className: 'text-left' },
                { data: 'dataType', className: 'text-center' },
                { data: 'length', className: 'text-center' },
                { data: 'isNull', className: 'text-center' },
                {
                    data: 'rowDesc',
                    className: 'text-left',
                    render: function (data, type, row) {
                        const safeData = self.htmlEncode(data);
                        return `<textarea class="form-control form-control-sm col-desc-input" 
                                   data-id="${row.rowId}" 
                                   data-original="${safeData}" 
                                   rows="1">${safeData}</textarea>`;
                    }
                },
                {
                    data: 'rowRemark',
                    className: 'text-left',
                    render: function (data, type, row) {
                        const safeData = self.htmlEncode(data);
                        return `<textarea class="form-control form-control-sm col-remark-input" 
                                   data-id="${row.rowId}" 
                                   data-original="${safeData}" 
                                   rows="1">${safeData}</textarea>`;
                    }
                },
                {
                    data: 'rowName',
                    className: 'text-center',
                    render: function (data, type, row) {
                        const pureColName = data.replace(/<[^>]*>/g, "")
                            .replace(" PK", "")
                            .replace(" FK", "")
                            .trim();

                        return `<button type="button" class="btn btn-outline-info btn-sm" 
                        onclick="window.DIC1999R02.showLogs('${encodeURIComponent(pureColName)}')">
                    <i class="fa-solid fa-clock-rotate-left"></i>
                </button>`;
                    }
                },
                {
                    data: 'deleteAction',
                    className: 'text-center'
                }
            ],
            language: { url: self.urls.dataTableLangUrl }
        });

        self.bindEvents();
    },

    // 跳轉至操作紀錄頁面並帶入上下文參數
    showLogs: function (encodedColumnName) {
        const self = this;
        // 因為傳進來已經 encode 過了，這裡直接組合即可
        const url = `${self.urls.logUrl}?dbKey=${encodeURIComponent(self.urls.dbKey)}&tableName=${encodeURIComponent(self.urls.tableName)}&pkName=${encodedColumnName}&sid=${self.urls.sid}`;

        if (typeof loadMvc === 'function') {
            loadMvc(url);
        } else {
            window.location.href = url;
        }
    },

    bindEvents: function () {
        const self = this;

        // 搜尋
        $('#btnColumnSearch').off('click').on('click', function () {
            self.dt.ajax.reload();
        });

        $('#columnSearchKeyword').off('keypress').on('keypress', function (e) {
            if (e.which === 13) {
                e.preventDefault();
                self.dt.ajax.reload();
            }
        });

        // 快捷儲存：按下 Enter 觸發批次儲存
        $(document).off('keypress', '.col-desc-input, .col-remark-input').on('keypress', '.col-desc-input, .col-remark-input', function (e) {
            if (e.which === 13 && !e.shiftKey) {
                e.preventDefault();
                $('#btnSaveColumnBatch').click();
            }
        });

        // 新增欄位
        $('#btnConfirmCreateCol').off('click').on('click', function () {
            const data = {
                dbKey: self.urls.dbKey,
                tableName: self.urls.tableName,
                columnName: $('#newColName').val().trim(),
                dataType: $('#newColType').val(),
                length: parseInt($('#newColLength').val()) || 0,
                isNullable: $('#newColNullable').val() === 'true',
                description: $('#newColDesc').val().trim()
            };

            if (!data.columnName) return showError("請填寫欄位名稱");

            Swal.fire({ title: '結構變更中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

            $.ajax({
                url: `${self.urls.createColumn}?sid=${self.urls.sid}`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (res) {
                    Swal.close();
                    if (res.success) {
                        showAdminToast(res.message, "success");
                        $('#createColumnModal').modal('hide');
                        $('#newColName, #newColDesc').val('');
                        self.dt.ajax.reload();
                    } else {
                        showError(res.message);
                    }
                }
            });
        });

        // 刪除欄位
        $('#tableColumns').off('click', '.deleteColBtn').on('click', '.deleteColBtn', function () {
            const rowId = $(this).data('id');
            const colName = $(this).data('name');

            Swal.fire({
                title: '確定刪除？',
                text: `將永久移除欄位【${colName}】！`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                confirmButtonText: '確定',
                cancelButtonText: '取消'
            }).then((result) => {
                if (result.isConfirmed) {
                    $.post(`${self.urls.deleteColumn}?sid=${self.urls.sid}`, {
                        dbKey: self.urls.dbKey,
                        tableName: self.urls.tableName,
                        columnName: colName,
                        rowId: rowId
                    }, function (res) {
                        if (res.success) {
                            showAdminToast(res.message, "success");
                            self.dt.ajax.reload(null, false);
                        } else {
                            showError(res.message);
                        }
                    });
                }
            });
        });

        // 批次儲存
        $('#btnSaveColumnBatch').off('click').on('click', function () {
            const updates = [];
            const rowsMap = {};

            $('.col-desc-input, .col-remark-input').each(function () {
                const $this = $(this);
                const rowId = $this.data('id');
                const val = $this.val().trim();
                const original = String($this.data('original') || '').trim();

                if (!rowsMap[rowId]) rowsMap[rowId] = { rowId: rowId, changed: false };
                if ($this.hasClass('col-desc-input')) {
                    rowsMap[rowId].rowDesc = val;
                } else {
                    rowsMap[rowId].rowRemark = val;
                }
                if (val !== original) rowsMap[rowId].changed = true;
            });

            Object.values(rowsMap).forEach(r => { if (r.changed) updates.push(r); });
            if (updates.length === 0) return showAdminToast("無變更", "info");

            $.ajax({
                url: `${self.urls.updateFields}?dbKey=${encodeURIComponent(self.urls.dbKey)}&tableName=${encodeURIComponent(self.urls.tableName)}&sid=${self.urls.sid}`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(updates),
                success: (res) => {
                    if (res.success) {
                        showAdminToast(res.message, "success");
                        self.dt.ajax.reload(null, false);
                    } else showError(res.message);
                }
            });
        });
    }
};