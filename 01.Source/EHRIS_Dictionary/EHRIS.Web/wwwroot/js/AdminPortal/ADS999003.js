window.ADS999003 = {
    dt: null,
    init: function() {
        const self = this;
        const urls = window.userAction;

        $.ajaxPrefilter(function(options, originalOptions, jqXHR) {
            if (!options.crossDomain && urls.tokenValue) {
                jqXHR.setRequestHeader('RequestVerificationToken', urls.tokenValue);
            }
        });

self.dt = createEhrisTable('userTable', {
            ajaxUrl: urls.list,
            langUrl: urls.dataTableLangUrl,
            searchableCols: [
                { index: 1, label: '姓名' },
                { index: 2, label: '帳號' }
            ],
            columns: [
                { data: 'aduLogin' },
                { data: 'aduDisplayName' },
                { data: 'aduEmail' },
                { data: 'roleNames' },
                { 
                    data: 'aduStatus', 
                    className: 'text-center',
                    render: function(data) {
                        return data === 1 
                            ? '<span class="badge badge-enabled">啟用</span>' 
                            : '<span class="badge badge-disabled">不啟用</span>'; 
                    }
                },
                { data: 'aduModifyTime', className: 'text-center' },
                { 
                    data: 'editAction', 
                    className: 'text-center', 
                    orderable: false 
                },
                { 
                    data: 'delAction', 
                    className: 'text-center', 
                    orderable: false 
                }
            ],
            onInitComplete: function(api) {
                if (urls.hasAdd) {
                    const $wrapper = $(api.table().container());
                    const $rightTop = $wrapper.find('.row:first .col-md-6:last');
                    if ($rightTop.find('#btnAdd').length === 0) {
                        $rightTop.append(`<button id="btnAdd" class="btn btn-success btn-sm float-end">新增使用者</button>`);
                    }
                }
            }
        });

        $(document).off('click', '#btnAdd').on('click', '#btnAdd', () => self.open(0));
        $(document).off('click', '.editUser').on('click', '.editUser', function() { 
            self.open($(this).data('id')); 
        });
        $(document).off('click', '#btnSaveUser').on('click', '#btnSaveUser', () => self.save());
        $(document).off('click', '.deleteUser').on('click', '.deleteUser', function() { 
            self.remove($(this).data('id')); 
        });
    },

    open: function(id) {
        const self = this;
        const urls = window.userAction;
        
        Swal.fire({ title: '讀取中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });

        $.get(urls.data, { aduNo: id }, res => {
            Swal.close();
            if (!res.success) {
                ehrisAlert.error(res.message);
                return;
            }
            
            const d = res.data;
            $('#userModalLabel').text(id === 0 ? '新增使用者' : '修改使用者');
            
            $('#aduLogin').val(d.aduLogin || '').prop('readonly', id !== 0);
            $('#aduDisplayName').val(d.aduDisplayName || '');
            $('#aduEmail').val(d.aduEmail || '');
            $('#aduPassword').val(''); 
            $('#aduStatus').prop('checked', d.aduStatus === 1 || id === 0);
            $('#btnSaveUser').data('id', id);

            $('#adlNo').empty().append(res.levels.map(l => 
                `<option value="${l.adlNo}">${l.adlName}</option>`
                )); 
            $('#adlNo').val(d.adlNo || (res.levels.length > 0 ? res.levels[0].adlNo : ""));

            const roleIds = d.roleIds || [];
            $('#roleList').empty().append(res.roles.map(r => `
                <div class="form-check form-check-inline">
                    <input class="form-check-input" type="checkbox" value="${r.adrNo}" id="r${r.adrNo}" ${roleIds.includes(r.adrNo) ? 'checked' : ''}>
                    <label class="form-check-label" for="r${r.adrNo}">${r.adrRoleName}</label>
                </div>`));

            if (id === 0) {
                $('#pwdStar').show();
                $('#aduPassword').attr('placeholder', '新增時必填');
            } else {
                $('#pwdStar').hide();
                $('#aduPassword').attr('placeholder', '需變更請輸入新密碼');
            }
        
            $('#userModal').modal('show');
        }).fail(xhr => {
            Swal.close();
            ehrisAlert.handleError(xhr);
        });
    },

save: function() {
    const self = this;
    const id = $('#btnSaveUser').data('id');
    const roles = [];
    $('#roleList input:checked').each(function() { roles.push(parseInt($(this).val())); });
    
    const body = {
        AduNo: id, 
        AduLogin: $('#aduLogin').val().trim(),
        AduDisplayName: $('#aduDisplayName').val().trim(),
        AduEmail: $('#aduEmail').val().trim(),
        AduPassword: $('#aduPassword').val(),
        AdlNo: parseInt($('#adlNo').val()),
        AduStatus: $('#aduStatus').is(':checked') ? 1 : 0,
        RoleIds: roles
    };

    if (!body.AduLogin) return ehrisAlert.error('請輸入登入帳號');
    if (!body.AduDisplayName) return ehrisAlert.error('請輸入姓名');
    if (!body.AduEmail) return ehrisAlert.error('請輸入電子郵件');
    if (id === 0 && !body.AduPassword) return ehrisAlert.error('新增使用者時密碼為必填');
    if (isNaN(body.AdlNo) || body.AdlNo <= 0) return ehrisAlert.error('請選權限等級');
    if (body.RoleIds.length === 0) return ehrisAlert.error('請至少選擇一個角色');

    Swal.fire({ title: '儲存中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });

    $.ajax({
            url: window.userAction.save,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(body), 
            success: res => {
                Swal.close();
                if (res.success) {
                    $('#userModal').modal('hide');
                    self.dt.ajax.reload(null, false);
                }
                ehrisAlert.handle(res);
            },
            error: xhr => {
                Swal.close();
                ehrisAlert.handleError(xhr);
            }
        });
},

    remove: function(id) {
        const self = this;

        Swal.fire({
            title: `確定要刪除？`,
            text: `刪除後將無法復原！`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#5A2323',
            cancelButtonColor: '#A67C52',
            confirmButtonText: '確定',
            cancelButtonText: '取消'
        }).then(r => {
            if (r.isConfirmed) {
                Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
                $.ajax({
                    url: window.userAction.del,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ aduNo: id }),
                    success: res => {
                        Swal.close();
                        if (res.success) self.dt.ajax.reload(null, false);
                        ehrisAlert.handle(res);
                    },
                    error: xhr => {
                        Swal.close();
                        ehrisAlert.handleError(xhr);
                    }
                });
            }
        });
    }
};