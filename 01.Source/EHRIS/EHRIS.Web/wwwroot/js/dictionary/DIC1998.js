window.DIC1998 = {
    dt: null,
    urls: window.accessAction || {},

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
        const $table = $('#accessTable');

        if ($table.length === 0) return;

        if (!self.urls.getData) {
            console.warn("DIC1998: 設定未載入，取消初始化。");
            return;
        }

        const action = self.urls;
        $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
            if (!options.crossDomain && action.tokenValue) {
                jqXHR.setRequestHeader('RequestVerificationToken', action.tokenValue);
            }
        });

        if ($.fn.DataTable.isDataTable('#accessTable')) {
            $table.DataTable().clear().destroy();
            $table.empty();
        }

        self.dt = $table.DataTable({
            serverSide: true,
            ordering: false,
            processing: true,
            searching: false,
            ajax: {
                url: self.urls.getData,
                type: 'POST',
                contentType: 'application/json',
                data: function (d) {
                    return JSON.stringify({
                        draw: d.draw,
                        start: d.start,
                        length: d.length,
                        queryClientIp: $('#queryClientIp').val(),
                        queryServerIp: $('#queryServerIp').val(),
                        queryMenuId: $('#queryMenuId').val() ? parseInt($('#queryMenuId').val()) : null
                    });
                }
            },
            columns: [
                { data: 'clientIp', className: 'text-center' },
                { data: 'serverIp', className: 'text-center' },
                {
                    data: 'menus',
                    className: 'text-start',
                    render: function (data, type, row) {
                        if (!data || data.length === 0) return '';
                        return data.map(m => {
                            const encodedName = self.htmlEncode(m.menuName);
                            return `<span class="badge bg-info text-dark me-2 p-2" style="font-weight:500; font-size: 0.85rem;">
                                ${encodedName}
                                <i class="fas fa-times ms-2 text-danger delete-single-btn" 
                                   style="cursor:pointer;" 
                                   title="移除此授權"
                                   onclick="window.DIC1998.deleteItem(${m.accessId}, '${row.clientIp}', '${encodedName}')"></i>
                            </span>`;
                        }).join('');
                    }
                },
                { data: 'createDate', className: 'text-center' },
                {
                    data: null,
                    className: 'text-center',
                    render: function (data, type, row) {
                        return `<button type='button' class='btn btn-outline-primary btn-xs' 
                                 onclick="window.DIC1998.openModal(0, '${row.clientIp}', '${row.serverIp}', '')">
                                 新增授權
                                </button>`;
                    }
                }
            ],
            language: {
                url: self.urls.dataTableLangUrl
            }
        });

        self.bindEvents();
    },

    bindEvents: function () {
        const self = this;

        $(document).off('change', '#queryClientIp, #queryServerIp, #queryMenuId').on('change', '#queryClientIp, #queryServerIp, #queryMenuId', function () {
            self.dt.ajax.reload();
        });

        $(document).off('click', '#btnCreate').on('click', '#btnCreate', function () {
            self.openModal(0, '', '', '');
        });

        $(document).off('change', '#serverIpSelect').on('change', '#serverIpSelect', function () {
            const serverIp = $(this).val();
            self.loadMenus(serverIp, '');
        });

        $('#btnSaveAccess').off('click').on('click', function () {
            self.saveData();
        });
    },

    openModal: function (id, clientIp, serverIp, menuId) {
        const self = this;
        $('#modalAccessId').val(id);
        $('#clientIpInput').val(clientIp);

        if (id === 0) {
            $('#accessModalLabel').text(clientIp ? `為 ${clientIp} 新增授權` : '新增 IP 授權');
            if (serverIp) {
                $('#serverIpSelect').val(serverIp);
                self.loadMenus(serverIp, menuId);
            } else {
                $('#serverIpSelect').val('');
                $('#menuIdSelect').empty().append('<option value="">請先選擇伺服器</option>');
            }
        }
        $('#accessModal').modal('show');
    },

    loadMenus: function (serverIp, selectedMenuId) {
        const $menuSelect = $('#menuIdSelect');
        $menuSelect.empty().append('<option value="">載入中...</option>');

        if (!serverIp) {
            $menuSelect.empty().append('<option value="">請先選擇伺服器</option>');
            return;
        }

        $.ajax({
            url: this.urls.getMenus,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ serverIp: serverIp }),
            success: function (res) {
                $menuSelect.empty();
                if (res.success && res.data.length > 0) {
                    $menuSelect.append('<option value="">請選擇授權資料庫</option>');
                    res.data.forEach(function (item) {
                        const isSelected = (item.value.toString() === selectedMenuId.toString()) ? 'selected' : '';
                        $menuSelect.append(`<option value="${item.value}" ${isSelected}>${item.text}</option>`);
                    });
                } else {
                    $menuSelect.append('<option value="">此伺服器無可用資料庫</option>');
                }
            }
        });
    },

    saveData: function () {
        const self = this;
        const accessId = $('#modalAccessId').val();
        const serverIp = $('#serverIpSelect').val();
        const menuId = $('#menuIdSelect').val();
        const clientIp = $('#clientIpInput').val().trim();

        if (!serverIp || !menuId || !clientIp) {
            ehrisAlert.warning('請填寫所有必填欄位');
            return;
        }

        Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

        const postData = {
            AccessId: parseInt(accessId),
            ClientIp: clientIp,
            MenuId: parseInt(menuId)
        };

        $.ajax({
            url: self.urls.save,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(postData),
            success: function (res) {
                Swal.close();
                ehrisAlert.handle(res).then(function () {
                    if (res.success) {
                        $('#accessModal').modal('hide');
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

    deleteItem: function (id, ip, menuName) {
        const self = this;
        Swal.fire({
            title: '確定移除權限？',
            html: `確定要移除 IP【${ip}】對資料庫<br><b>【${menuName}】</b>的存取授權嗎？`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: '是的，移除！',
            cancelButtonText: '取消'
        }).then((result) => {
            if (result.isConfirmed) {
                Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });
                $.ajax({
                    url: self.urls.delete,
                    type: 'POST',
                    data: { accessId: id },
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