window.DIC1999R01 = {
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

        self.dt = createEhrisTable('tableTables', {
            ajaxUrl: `${self.urls.getData}?sid=${self.urls.sid}`,
            langUrl: self.urls.dataTableLangUrl,
            extraData: function (d) {
                return { dbKey: self.urls.dbKey, sid: self.urls.sid };
            },
            columns: [
                {
                    data: 'tableName',
                    render: function (data, type, row) {
                        const safeData = self.htmlEncode(data);
                        const detailUrl = `${self.urls.columnList}?dbKey=${encodeURIComponent(self.urls.dbKey)}&tableName=${encodeURIComponent(data)}&sid=${self.urls.sid}`;
                        return `<a href="javascript:void(0);" onclick="loadMvc('${detailUrl}')" class="text-primary fw-bold">${safeData}</a>`;
                    }
                },
                {
                    data: 'sheetDesc',
                    render: function (data, type, row) {
                        const safeData = self.htmlEncode(data);
                        return `<input type="text" class="form-control form-control-sm table-desc-input" 
                               data-id="${row.sheetId}" data-name="${self.htmlEncode(row.tableName)}"
                               data-original="${safeData}" value="${safeData}" />`;
                    }
                },
                {
                    data: 'editAction',
                    className: 'text-center'
                },
                {
                    data: 'tableName',
                    className: 'text-center',
                    render: function (data, type, row) {
                        return `<button type="button" class="btn btn-outline-info btn-sm" 
                                        onclick="window.DIC1999R01.showLogs('${encodeURIComponent(data)}')">
                                    <i class="fa-solid fa-clock-rotate-left"></i>
                                </button>`;
                    }
                },
                {
                    data: 'deleteAction',
                    className: 'text-center'
                }
            ]
        });

        self.bindEvents();
    },

    showLogs: function (tableName) {
        const self = this;
        const url = `${self.urls.logUrl}?dbKey=${encodeURIComponent(self.urls.dbKey)}&tableName=${tableName}&sid=${self.urls.sid}`;

        if (typeof loadMvc === 'function') {
            loadMvc(url);
        } else {
            window.location.href = url;
        }
    },

    bindEvents: function () {
        const self = this;

        $('#tableTables').off('click', '.renameBtn').on('click', '.renameBtn', function () {
            const id = $(this).data('id');
            const name = $(this).data('name');
            $('#renameSheetId').val(id);
            $('#renameOldName').val(name);
            $('#renameNewName').val(name);
            $('#renameModal').modal('show');
        });

        $('#btnConfirmRename').off('click').on('click', function () {
            const id = $('#renameSheetId').val();
            const oldName = $('#renameOldName').val();
            const newName = $('#renameNewName').val().trim();

            if (!newName || oldName === newName) {
                $('#renameModal').modal('hide');
                return;
            }

            Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

            $.ajax({
                url: `${self.urls.renameTable}?sid=${self.urls.sid}`,
                type: 'POST',
                data: {
                    dbKey: self.urls.dbKey,
                    sheetId: id,
                    oldName: oldName,
                    newName: newName,
                    sid: self.urls.sid
                },
                success: function (res) {
                    Swal.close();
                    ehrisAlert.handle(res).then(function () {
                        if (res.success) {
                            $('#renameModal').modal('hide');
                            self.dt.ajax.reload(null, false);
                        }
                    });
                },
                error: function (xhr) {
                    Swal.close();
                    ehrisAlert.handleError(xhr);
                }
            });
        });

        $('#btnConfirmCreate').off('click').on('click', function () {
            const data = {
                dbKey: self.urls.dbKey,
                tableName: $('#newTableName').val().trim(),
                description: $('#newTableDesc').val().trim(),
                pkName: $('#newPkName').val().trim(),
                pkType: $('#newPkType').val(),
                pkIdentity: $('#newPkIdentity').val() === 'true',
                pkDescription: $('#newPkDesc').val().trim()
            };

            if (!data.tableName) {
                ehrisAlert.warning("請填寫資料表名稱");
                return;
            }

            Swal.fire({ title: '建立中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

            $.ajax({
                url: `${self.urls.createTable}?sid=${self.urls.sid}`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (res) {
                    Swal.close();
                    ehrisAlert.handle(res).then(function () {
                        if (res.success) {
                            $('#createTableModal').modal('hide');
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

        $('#tableTables').off('click', '.deleteBtn').on('click', '.deleteBtn', function () {
            const id = $(this).data('id');
            const name = $(this).data('name');

            Swal.fire({
                title: '確定刪除？',
                text: `將永久刪除實體表【${name}】及其所有描述紀錄！`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: '確定',
                cancelButtonText: '取消'
            }).then((result) => {
                if (result.isConfirmed) {
                    Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });
                    $.ajax({
                        url: `${self.urls.deleteTable}?sid=${self.urls.sid}`,
                        type: 'POST',
                        data: { dbKey: self.urls.dbKey, tableName: name, sheetId: id },
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

        $('#btnSaveTableBatchDesc').off('click').on('click', function () {
            const updates = [];
            $('.table-desc-input').each(function () {
                const $this = $(this);
                if ($this.val().trim() !== String($this.data('original') || '').trim()) {
                    updates.push({
                        sheetId: parseInt($this.data('id')),
                        tableName: $this.data('name'),
                        sheetDesc: $this.val().trim()
                    });
                }
            });

            if (updates.length === 0) return ehrisAlert.info("無任何變更");

            Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

            $.ajax({
                url: `${self.urls.updateDesc}?dbKey=${encodeURIComponent(self.urls.dbKey)}&sid=${self.urls.sid}`,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(updates),
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
        });

        $('#btnConfirmImport').off('click').on('click', function () {
            const fileInput = document.getElementById('importDictFile');
            if (!fileInput.files || fileInput.files.length === 0) {
                ehrisAlert.warning("請先選擇檔案");
                return;
            }

            const file = fileInput.files[0];
            const fileName = file.name.toLowerCase();
            if (!fileName.endsWith('.json')) {
                ehrisAlert.warning("僅允許上傳 .json 格式的檔案");
                return;
            }

            const formData = new FormData();
            formData.append('file', file);
            formData.append('dbKey', self.urls.dbKey);
            formData.append('sid', self.urls.sid);

            Swal.fire({ title: '匯入中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

            $.ajax({
                url: `${self.urls.importDict}?sid=${self.urls.sid}`,
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (res) {
                    Swal.close();
                    ehrisAlert.handle(res).then(function () {
                        if (res.success) {
                            $('#importDictModal').modal('hide');
                            fileInput.value = '';
                            self.dt.ajax.reload(null, false);
                        }
                    });
                },
                error: function (xhr) {
                    Swal.close();
                    ehrisAlert.handleError(xhr);
                }
            });
        });
    }
};