window.SYS202005 = {
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
            { index: 0, label: '代碼' },
            { index: 1, label: '職等' }
        ];

        const adjustColumnWidths = () => {
            if (self.dt) {
                self.dt.columns.adjust().draw(false);
            }
        };

        $(window).on('resize', function () {
            clearTimeout(self.resizeTimer);
            self.resizeTimer = setTimeout(adjustColumnWidths, 50);
        });

        $(document).on('hidden.bs.modal', '#itemModal', function () {
            setTimeout(function () {
                adjustColumnWidths();
                $(window).trigger('resize');
            }, 100);
        });

        self.dt = createEhrisTable('itemTable', {
            ajaxUrl: action.GetList,
            langUrl: action.SetLang,
            searchableCols: searchableCols,
            order: [[0, 'asc']],
            columns: [
                { data: 'pleCode', className: 'dt-col-min' },
                { data: 'pleName', className: 'dt-col-mid' },
                { data: 'pleModifyName', className: 'dt-col-mod', orderable: false },
                { data: 'pleModifyTimeDisplay', className: 'dt-col-time' },
                { data: 'editAction', className: 'text-center dt-col-action', orderable: false },
                { data: 'delAction', className: 'text-center dt-col-action', orderable: false }
            ],
            onInitComplete: function (api) {
                if (action.hasInsertPermission) {
                    const $wrapper = $(api.table().container());
                    const $rightTop = $wrapper.find('.row:first .col-md-6:last');
                    $rightTop.append(`
                        <div class="d-flex justify-content-end align-items-center gap-2">
                            <button id="btnAddItem" type="button" class="btn btn-success btn-sm" style="min-width:85px;">新增職等</button>
                        </div>`);
                }
                adjustColumnWidths();
            }
        });

        function saveItem(url, model, successCallback) {
            Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });

            $.ajax({
                url: url,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(model),
                success: function (res) {
                    Swal.close();
                    ehrisAlert.handle(res).then(function () {
                        if (res.success && successCallback) successCallback(res);
                    });
                },
                error: function (xhr) {
                    Swal.close();
                    ehrisAlert.handleError(xhr);
                }
            });
        }

        function deleteItem(id, name, callback) {
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
                        url: action.Delete,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ PleNo: id }),
                        success: res => {
                            Swal.close();
                            ehrisAlert.handle(res).then(function () {
                                if (res.success && callback) callback(res);
                            });
                        },
                        error: xhr => {
                            Swal.close();
                            ehrisAlert.handleError(xhr);
                        }
                    });
                }
            });
        }

        /* 事件綁定：新增職等 */
        $(document).on('click', '#btnAddItem', function () {
            const form = $('#itemForm');
            form[0].reset();
            $('#itemModalLabel').text('新增職等');
            $('#modal-mode-indicator').addClass('text-danger').removeClass('text-muted').text('(*為必填寫欄位)');
            $('#ple_code').prop('readonly', false);
            $('#btnSaveItem').data('id', null);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('itemModal')).show();
        });

        /* 事件綁定：修改職等 */
        $('#itemTable').on('click', 'tbody .editItem', function () {
            const id = $(this).data('id');
            Swal.fire({ title: '讀取中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });

            $.get(action.GetByNo, { pleNo: id })
                .done(res => {
                    Swal.close();
                    if (res.success) {
                        const form = $('#itemForm');
                        form[0].reset();
                        $('#itemModalLabel').text('修改職等');
                        $('#modal-mode-indicator').removeClass('text-danger').addClass('text-muted').text('(編輯模式)');
                        form.find('#ple_code').val(res.data.pleCode).prop('readonly', true);
                        form.find('#ple_name').val(res.data.pleName);
                        $('#btnSaveItem').data('id', res.data.pleNo);
                        bootstrap.Modal.getOrCreateInstance(document.getElementById('itemModal')).show();
                    } else {
                        ehrisAlert.error(res.message);
                    }
                })
                .fail(xhr => ehrisAlert.handleError(xhr));
        });

        /* 事件綁定：刪除職等 */
        $('#itemTable').on('click', 'tbody .deleteItem', function () {
            const id = $(this).data('id');
            const name = $(this).closest('tr').find('td:eq(1)').text();
            deleteItem(id, name, () => self.dt.ajax.reload(null, false));
        });

        /* 事件綁定：儲存按鈕 */
        $('#btnSaveItem').on('click', function () {
            const form = $('#itemForm');
            const id = $(this).data('id');
            const model = {
                PleNo: id ?? 0,
                PleCode: form.find('#ple_code').val().trim(),
                PleName: form.find('#ple_name').val().trim()
            };

            let isValid = true;
            let errMsg = "";

            if (!model.PleCode) { errMsg += '代碼不可為空！<br>'; isValid = false; }
            if (!model.PleName) { errMsg += '職等不可為空！<br>'; isValid = false; }

            /* 新增時的代碼長度補齊與驗證 */
            if (model.PleNo === 0 && model.PleCode) {
                if (model.PleCode.length > 0 && model.PleCode.length < 3) {
                    model.PleCode = model.PleCode.padStart(3, '0');
                    form.find('#ple_code').val(model.PleCode);
                }
                if (model.PleCode.length !== 3) {
                    errMsg += '新增時，代碼長度必須為 3 位數！<br>';
                    isValid = false;
                }
            }

            if (!isValid) {
                ehrisAlert.warning(errMsg);
                return;
            }

            const url = (id && id > 0) ? action.Update : action.Add;
            saveItem(url, model, function () {
                bootstrap.Modal.getInstance(document.getElementById('itemModal')).hide();
                self.dt.ajax.reload(null, false);
            });
        });
    }
};