window.SYS202004 = {
    dt: null,
    action: null,
    resizeTimer: null,

    init: function () {
        const self = this;
        const action = window.permissionAction;
        self.action = action;

        $(document).off('hidden.bs.modal', '#professModal');
        $(document).off('click', '.editProfess, .deleteProfess, #btnAddProfess');
        $('#btnSave').off('click');

        $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
            if (!options.crossDomain && action.tokenValue) {
                jqXHR.setRequestHeader('RequestVerificationToken', action.tokenValue);
            }
        });

        const adjustColumnWidths = () => {
            if (self.dt) self.dt.columns.adjust().draw(false);
        };

        $(window).on('resize', () => {
            clearTimeout(self.resizeTimer);
            self.resizeTimer = setTimeout(adjustColumnWidths, 50);
        });

        $(document).on('hidden.bs.modal', '#professModal', () => {
            setTimeout(() => { adjustColumnWidths(); $(window).trigger('resize'); }, 100);
        });

        const searchableCols = [
            { index: 0, label: '代號' },
            { index: 1, label: '職稱中文' },
            { index: 2, label: '職稱英文' },
            { index: 5, label: '修建者' }
        ];

        self.dt = createEhrisTable('professTable', {
            ajaxUrl: action.getProfessData,
            searchableCols: searchableCols,
            order: [[4, 'asc']],
            columns: [
                { data: 'proCode', className: 'dt-col-min' },
                { data: 'proName', className: 'dt-col-mid' },
                { data: 'proEnglish', className: 'dt-col-mid' },
                {
                    data: 'proIsManager',
                    className: 'text-center dt-col-min',
                    render: function (d) {
                        return d === '是' ? '<strong>是</strong>' : '否';
                    }
                },
                { data: 'proOrder', className: 'text-center dt-col-order' },
                { data: 'proModifyName', className: 'dt-col-mod', orderable: false },
                { data: 'proModifyTime', className: 'dt-col-time' },
                { data: 'editAction', className: 'text-center dt-col-action', orderable: false },
                { data: 'deleteAction', className: 'text-center dt-col-action', orderable: false }
            ],
            onInitComplete: function (api) {
                if (action.hasInsertPermission) {
                    const $wrapper = $(api.table().container());
                    const $rightTop = $wrapper.find('.row:first .col-md-6:last');
                    $rightTop.append(`
                        <div class="d-flex justify-content-end align-items-center gap-2">
                            <button id="btnAddProfess" type="button" class="btn btn-success btn-sm" style="min-width:90px;">新增職稱</button>
                        </div>`);
                }
                adjustColumnWidths();
            }
        });

        const showModal = (id, title, isEdit) => {
            Swal.fire({ title: '讀取中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
            $.get(action.getProfessForEdit, { id: id }).done(res => {
                Swal.close();
                if (!res.success) {
                    ehrisAlert.error(res.message);
                    return;
                }

                const form = $('#professForm');
                form[0].reset();
                $('#modalLabel').text(title);
                $('#modal-mode-indicator').text(isEdit ? '(編輯模式)' : '(*為必填寫欄位)')
                    .toggleClass('text-muted', isEdit).toggleClass('text-danger', !isEdit);

                form.find('#proNo').val(res.data.proNo || 0);
                form.find('#proCode').val(res.data.proCode || '').prop('readonly', isEdit);
                form.find('#proName').val(res.data.proName || '');
                form.find('#proEnglish').val(res.data.proEnglish || '');
                form.find('#proOrder').val(res.data.proOrder || 0);
                form.find(`input[name="proIsManager"][value="${res.data.proIsManager || '0'}"]`).prop('checked', true);

                bootstrap.Modal.getOrCreateInstance(document.getElementById('professModal')).show();
            }).fail(xhr => ehrisAlert.handleError(xhr));
        };

        $(document).on('click', '#btnAddProfess', () => showModal(0, '新增職稱', false));
        $(document).on('click', '.editProfess', function () { showModal($(this).data('id'), '修改職稱', true); });

        $('#btnSave').on('click', function () {
            const form = $('#professForm');
            const id = parseInt(form.find('#proNo').val() || 0);
            const model = {
                ProNo: id,
                ProCode: form.find('#proCode').val().trim(),
                ProName: form.find('#proName').val().trim(),
                ProEnglish: form.find('#proEnglish').val().trim(),
                ProIsManager: form.find('input[name="proIsManager"]:checked').val(),
                ProOrder: parseInt(form.find('#proOrder').val())
            };

            let isValid = true;
            let errMsg = "";
            if (!model.ProCode) { errMsg += '代號為必填項！<br>'; isValid = false; }
            if (!model.ProName) { errMsg += '職稱中文為必填項！<br>'; isValid = false; }
            if (isNaN(model.ProOrder)) { errMsg += '排序為必填項！<br>'; isValid = false; }

            if (id === 0 && model.ProCode.length > 0 && model.ProCode.length < 4) {
                model.ProCode = model.ProCode.padStart(4, '0');
                form.find('#proCode').val(model.ProCode);
            }

            if (!isValid) {
                ehrisAlert.warning(errMsg);
                return;
            }

            Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
            $.ajax({
                url: id === 0 ? action.createProfess : action.updateProfess,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(model),
                success: function (res) {
                    Swal.close();
                    ehrisAlert.handle(res).then(function () {
                        if (res.success) {
                            $('#professModal').modal('hide');
                            loadMvc(action.pageUrl);
                        }
                    });
                },
                error: xhr => {
                    Swal.close();
                    ehrisAlert.handleError(xhr);
                }
            });
        });

        $(document).on('click', '.deleteProfess', function () {
            const id = $(this).data('id');
            const name = $(this).closest('tr').find('td:eq(1)').text();

            Swal.fire({
                title: `確定要刪除【${name}】？`,
                text: `刪除後將無法復原！`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: '確定',
                cancelButtonText: '取消'
            }).then(result => {
                if (result.isConfirmed) {
                    Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
                    $.ajax({
                        url: action.deleteProfess,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ id: id }),
                        success: function (res) {
                            Swal.close();
                            ehrisAlert.handle(res).then(function () {
                                if (res.success) loadMvc(action.pageUrl);
                            });
                        },
                        error: xhr => {
                            Swal.close();
                            ehrisAlert.handleError(xhr);
                        }
                    });
                }
            });
        });
    }
};