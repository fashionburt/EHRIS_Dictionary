window.DIC1999R02 = {
    dt: null,
    urls: null,

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
        const self = this;
        self.urls = window.permissionAction;
        const action = self.urls;

        $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
            if (!options.crossDomain && action.tokenValue) {
                jqXHR.setRequestHeader('RequestVerificationToken', action.tokenValue);
            }
        });

        self.dt = createEhrisTable('tableColumns', {
            ajaxUrl: `${self.urls.getData}?sid=${self.urls.sid}`,
            langUrl: self.urls.dataTableLangUrl,
            extraData: function (d) {
                return { dbKey: self.urls.dbKey, tableName: self.urls.tableName, sid: self.urls.sid };
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
                    className: 'text-center',
                    orderable: false
                }
            ]
        });

        self.bindEvents();
    },

    showLogs: function (encodedColumnName) {
        const self = this;
        const url = `${self.urls.logUrl}?dbKey=${encodeURIComponent(self.urls.dbKey)}&tableName=${encodeURIComponent(self.urls.tableName)}&pkName=${encodedColumnName}&sid=${self.urls.sid}`;

        if (typeof loadMvc === 'function') {
            loadMvc(url);
        } else {
            window.location.href = url;
        }
    },

    bindEvents: function () {
        const self = this;

        $(document).off('keypress', '.col-desc-input, .col-remark-input').on('keypress', '.col-desc-input, .col-remark-input', function (e) {
            if (e.which === 13 && !e.shiftKey) {
                e.preventDefault();
                $('#btnSaveColumnBatch').click();
            }
        });

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

            if (!data.columnName) return ehrisAlert.warning("請填寫欄位名稱");

            Swal.fire({ title: '結構變更中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

            $.ajax({
                url: `${self.urls.createColumn}?sid=${self.urls.sid}`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (res) {
                    Swal.close();
                    ehrisAlert.handle(res).then(function () {
                        if (res.success) {
                            $('#createColumnModal').modal('hide');
                            $('#newColName, #newColDesc').val('');
                            self.dt.ajax.reload();
                        }
                    });
                },
                error: function (xhr) {
                    Swal.close();
                    ehrisAlert.handleError(xhr);
                }
            });
        });

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
                    Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });
                    $.ajax({
                        url: `${self.urls.deleteColumn}?sid=${self.urls.sid}`,
                        type: 'POST',
                        data: {
                            dbKey: self.urls.dbKey,
                            tableName: self.urls.tableName,
                            columnName: colName,
                            rowId: rowId
                        },
                        success: function (res) {
                            Swal.close();
                            ehrisAlert.handle(res).then(function () {
                                if (res.success) {
                                    self.dt.ajax.reload(null, false);
                                }
                            });
                        },
                        error: function (xhr) {
                            Swal.close();
                            ehrisAlert.handleError(xhr);
                        }
                    });
                }
            });
        });

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
            if (updates.length === 0) return ehrisAlert.info("無變更");

            Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

            $.ajax({
                url: `${self.urls.updateFields}?dbKey=${encodeURIComponent(self.urls.dbKey)}&tableName=${encodeURIComponent(self.urls.tableName)}&sid=${self.urls.sid}`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(updates),
                success: (res) => {
                    Swal.close();
                    ehrisAlert.handle(res).then(function () {
                        if (res.success) {
                            self.dt.ajax.reload(null, false);
                        }
                    });
                },
                error: (xhr) => {
                    Swal.close();
                    ehrisAlert.handleError(xhr);
                }
            });
        });
    }
};