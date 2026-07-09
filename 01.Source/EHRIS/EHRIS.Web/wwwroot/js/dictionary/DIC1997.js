window.DIC1997 = {
    dt: null,
    urls: window.permissionAction,
    isInitialTab: true,
    isInitialPk: true,

    init: function () {
        const self = this;
        const tableId = '#logTable';

        if ($.fn.DataTable.isDataTable(tableId)) {
            $(tableId).DataTable().destroy();
        }

        self.dt = $(tableId).DataTable({
            serverSide: true,
            processing: true,
            searching: false,
            ordering: true,
            ajax: {
                url: self.urls.getData,
                type: 'POST',
                contentType: 'application/json',
                data: function (d) {
                    let currentSid = $('#selSid').val();
                    let currentDb = $('#selDbKey').val();
                    let currentTab = $('#selTableName').val();
                    let currentPk = $('#selPkName').val();

                    if (self.isInitialTab && $('#initTableName').val()) {
                        currentTab = $('#initTableName').val();
                    }
                    if (self.isInitialPk && $('#initPkName').val()) {
                        currentPk = $('#initPkName').val();
                    }

                    this.url = `${self.urls.getData}?sid=${currentSid}`;

                    return JSON.stringify({
                        draw: d.draw,
                        start: d.start,
                        length: d.length,
                        orderby: d.order,
                        columns: d.columns,
                        dbKey: currentDb,
                        tableName: currentTab,
                        pkName: currentPk,
                        state: $('#selState').val(),
                        sid: currentSid,
                        extraSearch: {
                            searchValue: $('#txtKeyword').val()
                        }
                    });
                }
            },
            columns: [
                { data: 'dbKey', className: 'text-center' },
                { data: 'tableName', className: 'text-center' },
                { data: 'pkName', className: 'text-center' },
                { data: 'stateText', className: 'text-center' },
                { data: 'detail', className: 'text-left' },
                { data: 'dateText', className: 'text-center' }
            ],
            order: [[5, 'desc']],
            language: { url: self.urls.dataTableLangUrl }
        });

        self.bindEvents();

        if ($('#selDbKey').val()) {
            $('#selDbKey').trigger('change');
        }
    },

    bindEvents: function () {
        const self = this;

        $('#selSid').off('change').on('change', function () {
            self.isInitialTab = false;
            self.isInitialPk = false;
            $('#selDbKey').val('').trigger('change');
            self.dt.ajax.reload();
        });

        $('#btnSearch').off('click').on('click', function () {
            self.isInitialTab = false;
            self.isInitialPk = false;
            self.dt.ajax.reload();
        });

        $('#selDbKey').off('change').on('change', function () {
            const dbKey = $(this).val();
            const sid = $('#selSid').val();
            const $tabSel = $('#selTableName');
            const initTabName = $('#initTableName').val();

            if (!dbKey) {
                $tabSel.val('').empty().append('<option value="">全部</option>').prop('disabled', true);
                $('#selPkName').val('').empty().append('<option value="">全部</option>').prop('disabled', true);
                return;
            }

            $.get(self.urls.getTables, { dbKey: dbKey, sid: sid }, function (res) {
                $tabSel.empty().append('<option value="">全部</option>').prop('disabled', false);
                res.forEach(item => $tabSel.append(`<option value="${item}">${item}</option>`));

                if (self.isInitialTab && initTabName) {
                    $tabSel.val(initTabName);
                    $tabSel.trigger('change');
                }
            });
        });

        $('#selTableName').off('change').on('change', function () {
            const dbKey = $('#selDbKey').val();
            const tableName = $(this).val();
            const sid = $('#selSid').val();
            const $pkSel = $('#selPkName');

            let initPkName = $('#initPkName').val();
            if (initPkName) initPkName = initPkName.trim();

            if (!tableName) {
                $pkSel.val('').empty().append('<option value="">全部</option>').prop('disabled', true);
                return;
            }

            $.get(self.urls.getPks, { dbKey: dbKey, tableName: tableName, sid: sid }, function (res) {
                $pkSel.empty().append('<option value="">全部</option>').prop('disabled', false);

                res.forEach(item => {
                    const val = item ? item.trim() : "";
                    $pkSel.append(`<option value="${val}">${val}</option>`);
                });

                if (self.isInitialPk && initPkName) {
                    $pkSel.val(initPkName);

                    if ($pkSel.val() !== initPkName) {
                        console.warn("欄位名稱不匹配，無法自動選中:", initPkName);
                    }

                    self.isInitialTab = false;
                    self.isInitialPk = false;
                    $('#initTableName').val('');
                    $('#initPkName').val('');

                    self.dt.ajax.reload();
                }
            });
        });
    }
};