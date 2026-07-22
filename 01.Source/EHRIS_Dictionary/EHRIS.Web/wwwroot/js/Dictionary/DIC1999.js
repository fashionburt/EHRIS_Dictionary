window.DIC1999 = {
    dt: null,
    urls: window.permissionAction,

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
        const action = self.urls;

        $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
            if (!options.crossDomain && action.tokenValue) {
                jqXHR.setRequestHeader('RequestVerificationToken', action.tokenValue);
            }
        });

        self.dt = createEhrisTable('dbTable', {
            ajaxUrl: action.getData,
            langUrl: action.dataTableLangUrl,
            extraData: function (d) {
                return { serverIp: $('#filterServer').val() };
            },
            columns: [
                {
                    data: 'menuName',
                    className: 'text-left',
                    render: function (data, type, row) {
                        const safeData = self.htmlEncode(data);
                        const currentIp = $('#filterServer').val();
                        const sid = currentIp.split('.').pop();

                        const url = `${self.urls.tableList}?dbKey=${encodeURIComponent(data)}&sid=${sid}`;
                        return `<a href="javascript:void(0);" onclick="loadMvc('${url}')" class="text-primary fw-bold">${safeData}</a>`;
                    }
                },
                {
                    data: 'menuDesc',
                    className: 'text-left',
                    render: function (data, type, row) {
                        const safeData = self.htmlEncode(data);
                        return `<input type="text" class="form-control form-control-sm desc-input" 
                   data-id="${row.menuId}" 
                   data-original="${safeData}" 
                   value="${safeData}" />`;
                    }
                },
                { data: 'excelAction', className: 'text-center', orderable: false },
                { data: 'wordAction', className: 'text-center', orderable: false },
                { data: 'jsonAction', className: 'text-center', orderable: false },
                {
                    data: 'menuName',
                    className: 'text-center',
                    orderable: false,
                    render: function (data, type, row) {
                        return `<button type="button" class="btn btn-outline-info btn-sm" 
                            onclick="window.DIC1999.showLogs('${encodeURIComponent(data)}')">
                        <i class="fa-solid fa-clock-rotate-left"></i>
                    </button>`;
                    }
                },
                { data: 'deleteAction', className: 'text-center', orderable: false },
            ],
            onInitComplete: function (api) {
            }
        });

        self.bindEvents();
    },

    bindEvents: function () {
        const self = this;

        $('#filterServer').off('change').on('change', function () {
            self.dt.ajax.reload();
        });

        $(document).off('click', '#btnCreate').on('click', '#btnCreate', function () {
            const currentIp = $('#filterServer').val();
            self.openCreateModal(currentIp);
        });

        $(document).off('click', '#btnSaveDesc').on('click', '#btnSaveDesc', function () {
            self.saveDescriptions();
        });

        $(document).off('keypress', '.desc-input').on('keypress', '.desc-input', function (e) {
            if (e.which === 13) {
                self.saveDescriptions();
            }
        });

        $('#btnSaveDb').off('click').on('click', function () {
            self.saveNewDatabase();
        });

        $('#dbTable').off('click', '.exportExcel').on('click', '.exportExcel', function () {
            const id = $(this).data('id');
            const ip = $('#filterServer').val();
            self.downloadWithLoading(`${self.urls.exportExcel}?menuId=${id}&serverIp=${encodeURIComponent(ip)}`);
        });

        $('#dbTable').off('click', '.exportWord').on('click', '.exportWord', function () {
            const id = $(this).data('id');
            const ip = $('#filterServer').val();
            self.downloadWithLoading(`${self.urls.exportWord}?menuId=${id}&serverIp=${encodeURIComponent(ip)}`);
        });

        $('#dbTable').off('click', '.exportJson').on('click', '.exportJson', function () {
            const id = $(this).data('id');
            const ip = $('#filterServer').val();
            self.downloadWithLoading(`${self.urls.exportJson}?menuId=${id}&serverIp=${encodeURIComponent(ip)}`);
        });

        $('#dbTable').off('click', '.deleteBtn').on('click', '.deleteBtn', function () {
            const id = $(this).data('id');
            const name = $(this).data('name');
            const ip = $('#filterServer').val();
            self.deleteItem(id, name, ip);
        });

        $(document).off('change', '#dbSelect').on('change', '#dbSelect', function () {
            $('#menuName').val($(this).val());
        });
    },

    downloadWithLoading: function (url) {
        Swal.fire({
            title: '檔案產生中，請稍候...',
            allowOutsideClick: false,
            allowEscapeKey: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        fetch(url, { credentials: 'same-origin' })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error(`下載失敗（狀態碼 ${response.status}）`);
                }

                const disposition = response.headers.get('Content-Disposition') || '';
                let fileName = 'download';

                const starMatch = disposition.match(/filename\*=UTF-8''([^;]+)/i);
                if (starMatch && starMatch[1]) {
                    fileName = decodeURIComponent(starMatch[1]);
                } else {
                    const plainMatch = disposition.match(/filename="?([^";]+)"?/i);
                    if (plainMatch && plainMatch[1]) {
                        fileName = plainMatch[1];
                    }
                }

                return response.blob().then(function (blob) {
                    return { blob: blob, fileName: fileName };
                });
            })
            .then(function (result) {
                Swal.close();

                if (result.blob.size === 0) {
                    ehrisAlert.warning('沒有可匯出的資料');
                    return;
                }

                const blobUrl = window.URL.createObjectURL(result.blob);
                const a = document.createElement('a');
                a.href = blobUrl;
                a.download = result.fileName;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                window.URL.revokeObjectURL(blobUrl);
            })
            .catch(function (err) {
                Swal.close();
                ehrisAlert.error(err.message || '下載失敗，請稍後再試');
            });
    },

    showLogs: function (dbKey) {
        const currentIp = $('#filterServer').val();
        const sid = currentIp.split('.').pop();
        const url = `${this.urls.logUrl}?dbKey=${dbKey}&sid=${sid}`;

        if (typeof loadMvc === 'function') {
            loadMvc(url);
        } else {
            window.location.href = url;
        }
    },

    openCreateModal: function (serverIp) {
        const self = this;
        $('.field-validation').text('');
        $('#dbModalLabel').text(`新增資料庫描述 (${serverIp})`);
        $('#modalServerIp').val(serverIp);
        $('#dbSelect').empty().append('<option>載入中...</option>');
        $('#menuName').val('');
        $('#menuDesc').val('');
        $('#dbModal').modal('show');

        $.post(self.urls.getAvailableDatabases, { serverIp: serverIp }, function (res) {
            if (res.success) {
                const $select = $('#dbSelect');
                $select.empty().append('<option value="">請選擇...</option>');
                res.data.forEach(db => {
                    $select.append(`<option value="${db}">${db}</option>`);
                });
            }
        });
    },

    saveNewDatabase: function () {
        const self = this;
        const dbName = $('#menuName').val();
        const serverIp = $('#modalServerIp').val();
        if (!dbName) {
            $('[data-valmsg-for="menuName"]').text('請先選擇資料庫實體');
            return;
        }

        Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

        const body = {
            menuName: dbName,
            serverIP: serverIp,
            menuDesc: $('#menuDesc').val().trim(),
            isEnabled: 1
        };

        $.ajax({
            url: self.urls.addDesc,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(body),
            success: function (res) {
                Swal.close();
                ehrisAlert.handle(res).then(function () {
                    if (res.success) {
                        $('#dbModal').modal('hide');
                        self.dt.ajax.reload(null, false);
                    }
                });
            },
            error: function (xhr) {
                Swal.close();
                ehrisAlert.handleError(xhr);
            }
        });
    },

    saveDescriptions: function () {
        const self = this;
        const updates = [];
        const currentIp = $('#filterServer').val();

        $('.desc-input').each(function () {
            const $this = $(this);
            const currentVal = $this.val() ? $this.val().trim() : '';
            const originalVal = String($this.data('original') || '').trim();

            if (currentVal !== originalVal) {
                updates.push({
                    menuId: parseInt($this.data('id')),
                    serverIP: currentIp,
                    menuDesc: currentVal
                });
            }
        });

        if (updates.length === 0) {
            ehrisAlert.info("沒有任何變更");
            return;
        }

        Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

        $.ajax({
            url: self.urls.updateDesc,
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
    },

    deleteItem: function (id, name, ip) {
        const self = this;
        Swal.fire({
            title: '確定刪除？',
            text: `確定要刪除資料庫【${name}】的描述嗎？`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: '是的，刪除！',
            cancelButtonText: '取消'
        }).then((result) => {
            if (result.isConfirmed) {
                Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });
                $.ajax({
                    url: self.urls.delete,
                    type: 'POST',
                    data: { menuId: id, serverIp: ip },
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
    }
};