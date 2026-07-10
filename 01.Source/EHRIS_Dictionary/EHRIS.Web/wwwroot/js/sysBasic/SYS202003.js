window.SYS202003 = {
    dt: null,
    action: null,
    resizeTimer: null,

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
        const action = window.permissionAction;
        self.action = action;

        $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
            if (!options.crossDomain && action.tokenValue) {
                jqXHR.setRequestHeader('RequestVerificationToken', action.tokenValue);
            }
        });

        const searchableCols = [
            { index: 0, label: '代號' },
            { index: 1, label: '人員類別' },
            { index: 4, label: '修建者' }
        ];

        function adjustColumnWidths() {
            if (self.dt) {
                self.dt.columns.adjust().draw(false);
            }
        }

        $(window).on('resize', function () {
            clearTimeout(self.resizeTimer);
            self.resizeTimer = setTimeout(adjustColumnWidths, 50);
        });

        self.dt = createEhrisTable('ptypeTable', {
            ajaxUrl: action.getPTypeData,
            searchableCols: searchableCols,
            order: [[3, 'asc']],
            columns: [
                { data: 'ptyCode', className: 'dt-col-min' },
                { data: 'ptyName', className: 'dt-col-mid' },
                { data: 'personTypes', className: 'dt-col-max', orderable: false },
                { data: 'ptyOrder', className: 'text-center dt-col-order' },
                { data: 'ptyModifyName', className: 'dt-col-mod', orderable: false },
                { data: 'ptyModifyTime', className: 'dt-col-time' },
                { data: 'editAction', className: 'text-center dt-col-action', orderable: false },
                { data: 'deleteAction', className: 'text-center dt-col-action', orderable: false }
            ],
            onInitComplete: function (api) {
                if (action.hasInsertPermission) {
                    const $wrapper = $(api.table().container());
                    const $rightTop = $wrapper.find('.row:first .col-md-6:last');
                    $rightTop.append(`
                        <div class="d-flex justify-content-end align-items-center gap-2">
                            <button id="btnAddPType" type="button" class="btn btn-success btn-sm" style="min-width:110px;">
                                新增人員類別
                            </button>
                        </div>`);
                }
                adjustColumnWidths();
            }
        });

        function populateCheckboxes(allOptions, selectedCodes) {
            const container = $('#personTypesCheckboxes');
            container.empty();
            if (!allOptions || allOptions.length === 0) {
                container.html('<p class="text-muted">沒有可用的類別選項。</p>');
                return;
            }

            allOptions.forEach(option => {
                const isChecked = selectedCodes.includes(option.svrCode);
                const colDiv = document.createElement('div');
                colDiv.className = 'col-md-4 form-check';
                colDiv.innerHTML = `
                    <input class="form-check-input" type="checkbox" value="${option.svrCode}" id="chk_${option.svrCode}" ${isChecked ? 'checked' : ''}>
                    <label class="form-check-label" for="chk_${option.svrCode}">${option.svrName}</label>
                `;
                container[0].appendChild(colDiv);
            });
        }

        function getPTypeDetail(id, title, url, isEdit) {
            Swal.fire({ title: '讀取中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
            $.get(url, { id: id })
                .done(function (res) {
                    Swal.close();
                    if (res.success) {
                        const viewModel = res.data;
                        $('#ptypeForm')[0].reset();
                        $('#modalLabel').text(title);
                        const form = $('#ptypeForm');
                        form.find('#ptyNo').val(viewModel.ptyNo || 0);
                        form.find('#ptyCode').val(viewModel.ptyCode || '').prop('readonly', isEdit);
                        form.find('#ptyName').val(viewModel.ptyName || '');
                        form.find('#ptyOrder').val(viewModel.ptyOrder || 0);

                        populateCheckboxes(viewModel.allPersonTypes, viewModel.selectedPersonTypes || []);

                        bootstrap.Modal.getOrCreateInstance(document.getElementById('ptypeModal')).show();
                    } else {
                        ehrisAlert.error(res.message);
                    }
                })
                .fail(xhr => ehrisAlert.handleError(xhr));
        }

        $(document).on('click', '#btnAddPType', function () {
            getPTypeDetail(0, '新增人員類別', action.getPTypeForEdit, false);
        });

        $(document).on('click', '#ptypeTable tbody .editPType', function () {
            const id = $(this).data('id');
            getPTypeDetail(id, '修改人員類別', action.getPTypeForEdit, true);
        });

        $('#btnSave').on('click', function () {
            const form = $('#ptypeForm');
            const selectedTypes = $('#personTypesCheckboxes input:checked').map(function () { return $(this).val(); }).get();
            const model = {
                PtyNo: parseInt(form.find('#ptyNo').val() || 0),
                PtyCode: form.find('#ptyCode').val().trim(),
                PtyName: form.find('#ptyName').val().trim(),
                PtyOrder: parseInt(form.find('#ptyOrder').val()),
                SelectedPersonTypes: selectedTypes
            };

            let isValid = true;
            let errMsg = "";
            if (!model.PtyCode) { errMsg += '代號為必填項！<br>'; isValid = false; }
            if (!model.PtyName) { errMsg += '人員類別為必填項！<br>'; isValid = false; }
            if (isNaN(model.PtyOrder)) { errMsg += '排序為必填項！<br>'; isValid = false; }

            if (model.PtyNo === 0) {
                if (model.PtyCode.length > 0 && model.PtyCode.length < 2) {
                    model.PtyCode = model.PtyCode.padStart(2, '0');
                    form.find('#ptyCode').val(model.PtyCode);
                }
                if (model.PtyCode.length !== 2) {
                    errMsg += '新增時，代號長度必須為 2 位數！<br>'; isValid = false;
                }
            }

            if (!isValid) {
                ehrisAlert.warning(errMsg);
                return;
            }

            const url = model.PtyNo === 0 ? action.createPType : action.updatePType;
            Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });

            $.ajax({
                url: url,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(model),
                success: function (res) {
                    Swal.close();
                    ehrisAlert.handle(res).then(function () {
                        if (res.success) {
                            $('#ptypeModal').modal('hide');
                            loadMvc(action.pageUrl);
                        }
                    });
                },
                error: xhr => ehrisAlert.handleError(xhr)
            });
        });

        $(document).on('click', '#ptypeTable tbody .deletePType', function () {
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
                        url: action.deletePType,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ id: id }),
                        success: function (res) {
                            Swal.close();
                            ehrisAlert.handle(res).then(function () {
                                if (res.success) loadMvc(action.pageUrl);
                            });
                        },
                        error: xhr => ehrisAlert.handleError(xhr)
                    });
                }
            });
        });
    }
};