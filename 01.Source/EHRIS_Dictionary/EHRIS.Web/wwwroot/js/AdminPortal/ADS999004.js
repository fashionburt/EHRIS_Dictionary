window.ADS999004 = {
    dt: null,
    action: null,

    init: function () {
        const self = this;
        const action = window.permissionAction;
        self.action = action;

        $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
            if (!options.crossDomain && action.tokenValue) {
                jqXHR.setRequestHeader('RequestVerificationToken', action.tokenValue);
            }
        });

        const adjustColumnWidths = () => {
            if (self.dt) {
                self.dt.columns.adjust().draw(false);
            }
        };

        $(window).on('resize', adjustColumnWidths);
        $(document).on('hidden.bs.modal', '#itemModal', adjustColumnWidths);

        self.dt = createEhrisTable('itemTable', {
            ajaxUrl: action.GetList,
            langUrl: action.SetLang,
            order: [[0, 'asc']],
            columns: [
                { data: 'argVariable', className: 'dt-col-max' },
                { data: 'argDescribe', className: 'dt-col-max' },
                { data: 'argValue', className: 'dt-col-mid', orderable: false },
                { data: 'argDefaultValue', className: 'dt-col-mid', orderable: false },
                { data: 'argOpenManagerDisplay', className: 'text-center dt-col-min', orderable: false },
                { data: 'modifyName', className: 'dt-col-mod', orderable: false },
                { data: 'modifyTimeDisplay', className: 'dt-col-time' },
                { data: 'editAction', className: 'text-center dt-col-action', orderable: false },
                { data: 'delAction', className: 'text-center dt-col-action', orderable: false }
            ],
            onInitComplete: function (api) {
                if (action.hasInsertPermission) {
                    $('.row:first .col-md-6:last', $(api.table().container())).append(`
                        <div class="d-flex justify-content-end align-items-center">
                            <button id="btnAddItem" type="button" class="btn btn-success btn-sm">新增參數</button>
                        </div>`);
                }
            }
        });

        $(document).on('click', '#btnAddItem', function () {
            $('#itemForm')[0].reset();
            $('#itemModalLabel').text('新增系統參數');
            $('#agr_group').prop('disabled', false);
            $('#arg_variable').prop('readonly', false);
            $('#btnSaveItem').data('id', null);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('itemModal')).show();
        });

        $('#itemTable').on('click', 'tbody .editItem', function () {
            const id = $(this).data('id');
            Swal.fire({ title: '讀取中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
            $.get(action.GetByVariable, { variable: id }).done(res => {
                Swal.close();
                if (!res.success) return ehrisAlert.error(res.message);
                const f = $('#itemForm'), d = res.data;
                f[0].reset();
                $('#itemModalLabel').text('修改系統參數');

                const prefix = d.agrGroup + '_';
                f.find('#agr_group').val(d.agrGroup).prop('disabled', true);
                f.find('#arg_variable').val(d.argVariable.startsWith(prefix) ? d.argVariable.replace(prefix, '') : d.argVariable).prop('readonly', true);
                f.find('#arg_order').val(d.argOrder);
                f.find('#arg_describe').val(d.argDescribe);
                f.find('#arg_deatil').val(d.argDeatil);
                f.find('#arg_value').val(d.argValue);
                f.find('#arg_defaultvalue').val(d.argDefaultValue);
                f.find('#arg_source').val(d.argSource);
                f.find('#arg_splitchar').val(d.argSplitChar);
                f.find('#arg_multisel').prop('checked', d.argMultiSel === 1);
                f.find('#arg_openmanager').prop('checked', d.argOpenManager === 1);
                f.find('#arg_required').prop('checked', d.argRequired);
                $('#btnSaveItem').data('id', d.argVariable);
                bootstrap.Modal.getOrCreateInstance(document.getElementById('itemModal')).show();
            }).fail(xhr => { Swal.close(); ehrisAlert.handleError(xhr); });
        });

        $('#btnSaveItem').on('click', function () {
            const f = $('#itemForm'), cId = $(this).data('id');
            const fullVar = cId || (f.find('#agr_group').val() + '_' + f.find('#arg_variable').val().trim());

            const model = {
                ArgVariable: fullVar, AgrGroup: f.find('#agr_group').val(),
                ArgOrder: parseInt(f.find('#arg_order').val()) || 0, ArgDescribe: f.find('#arg_describe').val().trim(),
                ArgDeatil: f.find('#arg_deatil').val().trim(), ArgValue: f.find('#arg_value').val().trim(),
                ArgDefaultValue: f.find('#arg_defaultvalue').val().trim(), ArgSource: f.find('#arg_source').val(),
                ArgSplitChar: f.find('#arg_splitchar').val().trim(), ArgMultiSel: f.find('#arg_multisel').is(':checked') ? 1 : 0,
                ArgOpenManager: f.find('#arg_openmanager').is(':checked') ? 1 : 0, ArgRequired: f.find('#arg_required').is(':checked')
            };

            const url = cId ? action.Update : action.Add;
            Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
            $.ajax({
                url: url, type: 'POST', contentType: 'application/json', data: JSON.stringify(model),
                success: res => {
                    Swal.close();
                    ehrisAlert.handle(res).then(() => { if (res.success) { bootstrap.Modal.getInstance(document.getElementById('itemModal')).hide(); self.dt.ajax.reload(null, false); } });
                },
                error: xhr => { Swal.close(); ehrisAlert.handleError(xhr); }
            });
        });

        $('#itemTable').on('click', 'tbody .deleteItem', function () {
            const id = $(this).data('id');
            Swal.fire({
                title: `確定刪除【${id}】？`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: '確定',
                cancelButtonText: '取消'
            }).then(result => {
                if (result.isConfirmed) {
                    Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
                    $.ajax({
                        url: action.Delete,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ ArgVariable: id }),
                        success: res => {
                            Swal.close();
                            ehrisAlert.handle(res).then(() => { if (res.success) self.dt.ajax.reload(null, false); });
                        },
                        error: xhr => { Swal.close(); ehrisAlert.handleError(xhr); }
                    });
                }
            });
        });
    }
};