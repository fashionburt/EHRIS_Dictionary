window.ADS999001 = {
    dt: null,
    init: function () {
        const self = this;
        const urls = window.sysNoticeAction;

        $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
            if (!options.crossDomain && urls.tokenValue) {
                jqXHR.setRequestHeader('RequestVerificationToken', urls.tokenValue);
            }
        });

        self.dt = createEhrisTable('sysNoticeTable', {
            ajaxUrl: urls.list,
            langUrl: urls.dataTableLangUrl,
            order: [[2, 'desc']],
            searchableCols: [
                { index: 1, label: '公告內容' }
            ],
            extraData: function () {
                return {
                    filter: {
                        dateStart: $('#filterDateStart').val() || null,
                        dateEnd: $('#filterDateEnd').val() || null,
                        sysnType: $('#filterSysnType').val() === '' ? null : parseInt($('#filterSysnType').val())
                    }
                };
            },
            columns: [
                {
                    data: 'sysnType',
                    className: 'dt-col-min',
                    render: function (data, type, row) {
                        const map = {
                            0: '<span class="badge badge-urgent">緊急</span>',
                            1: '<span class="badge badge-important">重要</span>',
                            2: '<span class="badge badge-normal">一般</span>'
                        };
                        return map[data] || row.sysnTypeName;
                    }
                },
                { data: 'sysnContent', className: 'dt-col-max' },
                { data: 'sysnPublicDt', className: 'dt-col-mid' },
                { data: 'sysnStartTime', className: 'dt-col-mid' },
                { data: 'sysnEndTime', className: 'dt-col-mid' },
                {
                    data: 'sysnTop',
                    className: 'dt-col-action',
                    render: function (data) {
                        return data ? '<i class="fa fa-thumbtack text-danger"></i>' : '';
                    }
                },
                { data: 'editAction', className: 'dt-col-action', orderable: false },
                { data: 'delAction', className: 'dt-col-action', orderable: false }
            ],
            onInitComplete: function (api) {
                if (urls.hasAdd) {
                    const $wrapper = $(api.table().container());
                    const $rightTop = $wrapper.find('.row:first .col-md-6:last');
                    if ($rightTop.find('#btnAdd').length === 0) {
                        $rightTop.append(`<button id="btnAdd" class="btn btn-success btn-sm float-end">新增公告</button>`);
                    }
                }
            }
        });

        $(document).off('click', '#btnSearch').on('click', '#btnSearch', () => self.dt.ajax.reload());
        $(document).off('click', '#btnReset').on('click', '#btnReset', () => {
            $('#filterDateStart').val('');
            $('#filterDateEnd').val('');
            $('#filterSysnType').val('');
            self.dt.ajax.reload();
        });

        $(document).off('click', '#btnAdd').on('click', '#btnAdd', () => self.open(0));
        $(document).off('click', '.editSysNotice').on('click', '.editSysNotice', function () {
            self.open($(this).data('id'));
        });
        $(document).off('click', '#btnSaveSysNotice').on('click', '#btnSaveSysNotice', () => self.save());
        $(document).off('click', '.deleteSysNotice').on('click', '.deleteSysNotice', function () {
            self.remove($(this).data('id'));
        });
    },

    open: function (id) {
        const self = this;
        const urls = window.sysNoticeAction;

        Swal.fire({ title: '讀取中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });

        $.get(urls.data, { id: id }, res => {
            Swal.close();
            if (!res.success) {
                ehrisAlert.error(res.message);
                return;
            }

            const d = res.data;
            $('#sysNoticeModalLabel').text(id === 0 ? '新增公告' : '修改公告');
            $('#sysnNo').val(d.sysnNo || 0);
            $('#sysnType').val(d.sysnType != null ? String(d.sysnType) : '2');
            $('#sysnContent').val(d.sysnContent || '');
            $('#sysnPublicDt').val(d.sysnPublicDt ? String(d.sysnPublicDt).substring(0, 10) : '');
            $('#sysnStartTime').val(d.sysnStartTime ? String(d.sysnStartTime).substring(0, 16) : '');
            $('#sysnEndTime').val(d.sysnEndTime ? String(d.sysnEndTime).substring(0, 16) : '');
            $('#sysnTop').prop('checked', d.sysnTop === true);
            $('#btnSaveSysNotice').data('id', id);

            $('#sysNoticeModal').modal('show');
        }).fail(xhr => {
            Swal.close();
            ehrisAlert.handleError(xhr);
        });
    },

    save: function () {
        const self = this;
        const urls = window.sysNoticeAction;
        const id = $('#btnSaveSysNotice').data('id');

        const body = {
            SysnNo: id,
            SysnType: parseInt($('#sysnType').val()),
            SysnContent: ($('#sysnContent').val() || '').trim(),
            SysnPublicDt: $('#sysnPublicDt').val(),
            SysnStartTime: $('#sysnStartTime').val(),
            SysnEndTime: $('#sysnEndTime').val(),
            SysnTop: $('#sysnTop').is(':checked')
        };

        if (isNaN(body.SysnType)) return ehrisAlert.error('請選擇訊息等級');
        if (!body.SysnContent) return ehrisAlert.error('請輸入公告內容');
        if (!body.SysnPublicDt) return ehrisAlert.error('請選擇公告日期');
        if (!body.SysnStartTime) return ehrisAlert.error('請選擇顯示開始時間');
        if (!body.SysnEndTime) return ehrisAlert.error('請選擇顯示結束時間');
        if (new Date(body.SysnEndTime) < new Date(body.SysnStartTime)) return ehrisAlert.error('結束時間不可早於開始時間');

        Swal.fire({ title: '儲存中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });

        $.ajax({
            url: id === 0 ? urls.create : urls.update,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(body),
            success: res => {
                Swal.close();
                if (res.success) {
                    $('#sysNoticeModal').modal('hide');
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

    remove: function (id) {
        const self = this;
        const urls = window.sysNoticeAction;

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
                    url: urls.del,
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ SysnNo: id }),
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