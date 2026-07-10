window.SYS202006 = {
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
            { index: 1, label: '假別名稱' },
            { index: 5, label: '修建者' }
        ];

        const adjustColumnWidths = () => {
            if (self.dt) self.dt.columns.adjust().draw(false);
        };

        $(window).on('resize', function () {
            clearTimeout(self.resizeTimer);
            self.resizeTimer = setTimeout(adjustColumnWidths, 50);
        });

        $(document).on('hidden.bs.modal', '.modal', function () {
            setTimeout(function () {
                adjustColumnWidths();
                $(window).trigger('resize');
            }, 100);
        });

        self.dt = createEhrisTable('holidayTable', {
            ajaxUrl: action.getHolidayData,
            searchableCols: searchableCols,
            order: [[4, 'asc']],
            columns: [
                { data: 'holCode', className: 'dt-col-min' },
                { data: 'holName', className: 'dt-col-max' },
                {
                    data: 'holStatisticsDisplay', className: 'text-center dt-col-min', orderable: false,
                    render: data => data === '是' ? '<strong>是</strong>' : '否'
                },
                {
                    data: 'holOfficialDisplay', className: 'text-center dt-col-min', orderable: false,
                    render: data => data === '是' ? '<strong>是</strong>' : '否'
                },
                { data: 'holOrder', className: 'text-center dt-col-order' },
                { data: 'holModifyName', className: 'dt-col-mod', orderable: false },
                {
                    data: 'holModifyTime', className: 'dt-col-time',
                    render: data => {
                        if (!data) return '';
                        const date = new Date(data);
                        const y = date.getFullYear();
                        const m = ('0' + (date.getMonth() + 1)).slice(-2);
                        const d = ('0' + date.getDate()).slice(-2);
                        const h = ('0' + date.getHours()).slice(-2);
                        const min = ('0' + date.getMinutes()).slice(-2);
                        return `${y}-${m}-${d} ${h}:${min}`;
                    }
                },
                { data: 'editAction', className: 'text-center dt-col-action', orderable: false, searchable: false },
                { data: 'deleteAction', className: 'text-center dt-col-action', orderable: false, searchable: false }
            ],
            onInitComplete: function (api) {
                if (action.hasInsertPermission) {
                    const $wrapper = $(api.table().container());
                    const $rightTop = $wrapper.find('.row:first .col-md-6:last');
                    $rightTop.append(`
                        <div class="d-flex justify-content-end align-items-center gap-2">
                            <button id="btnAddHoliday" type="button" class="btn btn-success btn-sm" style="min-width:85px;">新增假別</button>
                        </div>`);
                }
                adjustColumnWidths();
            }
        });

        self.getHolidayDetail = function (id, title, url, isEdit) {
            Swal.fire({ title: '讀取中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
            $.get(url, { id: id })
                .done(function (res) {
                    Swal.close();
                    if (res.success) {
                        const viewModel = res.data;
                        $('#holidayForm')[0].reset();
                        $('#modalLabel').text(title);

                        const $indicator = $('#modal-mode-indicator');
                        if (isEdit) {
                            $indicator.text('(編輯模式)').removeClass('text-danger').addClass('text-muted');
                        } else {
                            $indicator.text('(*為必填寫欄位)').addClass('text-danger').removeClass('text-muted');
                        }

                        const form = $('#holidayForm');
                        form.find('#holNo').val(viewModel.holNo || 0);
                        form.find('#holCode').val(viewModel.holCode || '').prop('readonly', isEdit);
                        form.find('#holName').val(viewModel.holName || '');
                        form.find('#holOrder').val(viewModel.holOrder || 0);
                        form.find(`input[name="holStatistics"][value="${viewModel.holStatistics || '1'}"]`).prop('checked', true);
                        form.find(`input[name="holOfficial"][value="${viewModel.holOfficial || '0'}"]`).prop('checked', true);

                        bootstrap.Modal.getOrCreateInstance(document.getElementById('holidayModal')).show();
                    } else {
                        ehrisAlert.error(res.message || '資料讀取失敗。');
                    }
                })
                .fail(xhr => ehrisAlert.handleError(xhr));
        };

        $(document).on('click', '#btnAddHoliday', function () {
            self.getHolidayDetail(0, '新增假別', action.getHolidayForEdit, false);
        });

        $(document).on('click', '#holidayTable tbody .editHoliday', function () {
            const id = $(this).data('id');
            self.getHolidayDetail(id, '修改假別', action.getHolidayForEdit, true);
        });

        $('#btnSave').on('click', function () {
            const form = $('#holidayForm');
            const model = {
                HolNo: parseInt(form.find('#holNo').val() || 0),
                HolCode: form.find('#holCode').val().trim(),
                HolName: form.find('#holName').val().trim(),
                HolOrder: parseInt(form.find('#holOrder').val()),
                HolStatistics: form.find('input[name="holStatistics"]:checked').val(),
                HolOfficial: form.find('input[name="holOfficial"]:checked').val()
            };

            let isValid = true;
            let errMsg = "";
            if (!model.HolCode) { errMsg += '代號為必填項！<br>'; isValid = false; }
            if (!model.HolName) { errMsg += '假別名稱為必填項！<br>'; isValid = false; }
            if (isNaN(model.HolOrder)) { errMsg += '排序為必填項！<br>'; isValid = false; }
            if (model.HolStatistics === undefined) { errMsg += '是否統計為必填項！<br>'; isValid = false; }
            if (model.HolOfficial === undefined) { errMsg += '是否為公務假別為必填項！<br>'; isValid = false; }

            if (model.HolNo === 0 && model.HolCode.length > 0 && model.HolCode.length < 2) {
                model.HolCode = model.HolCode.padStart(2, '0');
                form.find('#holCode').val(model.HolCode);
            }

            if (!isValid) {
                ehrisAlert.warning(errMsg);
                return;
            }

            const url = model.HolNo === 0 ? action.createHoliday : action.updateHoliday;
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
                            $('#holidayModal').modal('hide');
                            self.dt.ajax.reload(null, false);
                        }
                    });
                },
                error: xhr => {
                    Swal.close();
                    ehrisAlert.handleError(xhr);
                }
            });
        });

        $(document).on('click', '#holidayTable tbody .deleteHoliday', function () {
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
                        url: action.deleteHoliday,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ id: id }),
                        success: function (res) {
                            Swal.close();
                            ehrisAlert.handle(res).then(function () {
                                if (res.success) self.dt.ajax.reload(null, false);
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