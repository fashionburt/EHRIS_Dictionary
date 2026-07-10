window.SYS202100 = {
    dt: null,
    currentMainArgValue: "",
    currentArgVariable: "",
    currentDepNo: 0,
    currentAgdValue: "",
    urls: window.permissionAction,
    depOptions: [],
    pendingSchedules: {},

    htmlEncode: function (value) {
        if (!value) return '';
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    },

    loadDeptGrid: function (argVariable) {
        const self = this;
        $.get(self.urls.GetDeptList, { argVariable: argVariable })
            .done(res => {
                if (res.success) {
                    const tbody = $('#deptGridTable tbody');
                    tbody.empty();
                    res.data.forEach(item => {
                        tbody.append(self.createDeptRowHtml(item));
                    });
                }
            });
    },

    createDeptRowHtml: function (item) {
        const self = this;
        let depFieldHtml = '';
        let schedBtnHtml = '';

        if (item.depNo > 0) {
            depFieldHtml = `<input type="text" class="form-control form-control-sm dep-name" value="${self.htmlEncode(item.depName)}" readonly>`;
            schedBtnHtml = `<button type="button" class="icon-btn text-primary openSched" data-depno="${self.htmlEncode(String(item.depNo))}" data-depname="${self.htmlEncode(item.depName)}"><i class="fa fa-calendar"></i></button>`;
        }

        return `
        <tr data-agdno="${self.htmlEncode(String(item.agdNo))}" data-depno="${self.htmlEncode(String(item.depNo))}">
            <td>${depFieldHtml}</td>
            <td><input type="text" class="form-control form-control-sm" value="${self.htmlEncode(item.argValue)}" readonly></td>
            <td><input type="text" class="form-control form-control-sm agd-value" value="${self.htmlEncode(item.agdValue)}"></td>
            <td class="text-center">${schedBtnHtml}</td>
            <td class="text-center">
                <button type="button" class="icon-btn text-danger btnDelDeptRow"><i class="fa fa-trash"></i></button>
            </td>
        </tr>`;
    },


    loadSchedGrid: function (argVariable, depNo) {
        const self = this;
        const tbody = $('#schedGridTable tbody');
        tbody.empty();

        if (self.pendingSchedules[depNo]) {
            self.pendingSchedules[depNo].forEach(item => {
                tbody.append(self.createSchedRowHtml(item));
            });
            return;
        }

        $.get(self.urls.GetSchedList, { argVariable: argVariable, depNo: depNo })
            .done(res => {
                if (res.success) {
                    self.pendingSchedules[depNo] = res.data;
                    res.data.forEach(item => {
                        tbody.append(self.createSchedRowHtml(item));
                    });
                }
            });
    },

    createSchedRowHtml: function (item) {
        const self = this;
        const startTime = item.agsStartTime ? self.htmlEncode(item.agsStartTime.substring(0, 16)) : '';
        const endTime = item.agsEndTime ? self.htmlEncode(item.agsEndTime.substring(0, 16)) : '';

        return `
        <tr data-agsno="${self.htmlEncode(String(item.agsNo))}">
            <td><input type="datetime-local" class="form-control form-control-sm start-time" value="${startTime}"></td>
            <td><input type="datetime-local" class="form-control form-control-sm end-time" value="${endTime}"></td>
            <td class="text-center">
                <button type="button" class="icon-btn text-danger btnDelSchedRow"><i class="fa fa-trash"></i></button>
            </td>
        </tr>`;
    },


    init: function () {
        const self = this;
        const urls = self.urls;

        $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
            if (!options.crossDomain && urls.tokenValue) {
                jqXHR.setRequestHeader('RequestVerificationToken', urls.tokenValue);
            }
        });

        $.get(urls.GetDepOptions).done(res => {
            if (res.success) {
                self.depOptions = res.data;
            }
        });

        self.dt = createEhrisTable('itemTable', {
            ajaxUrl: urls.GetList,
            langUrl: urls.SetLang,
            searchableCols: [
                { index: 0, label: '參數變數' },
                { index: 1, label: '參數說明' }
            ],
            order: [[0, 'asc']],
            columns: [
                { data: 'argVariable', className: 'dt-col-mid' },
                { data: 'argDescribe', className: 'dt-col-mid' },
                { data: 'argValue', className: 'dt-col-mid' },
                { data: 'deptAction', className: 'text-center dt-col-action', orderable: false },
                { data: 'editAction', className: 'text-center dt-col-action', orderable: false }
            ],
            onInitComplete: function () {
                if (self.dt) {
                    self.dt.columns.adjust().draw(false);
                }
            }
        });

        $(window).on('resize', function () {
            if (self.dt) {
                self.dt.columns.adjust().draw(false);
            }
        });

        $('#itemTable').on('click', 'tbody .editItem', function () {
            const id = $(this).data('id');
            Swal.fire({ title: '讀取中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });

            $.get(urls.GetByVariable, { argVariable: id })
                .done(res => {
                    Swal.close();
                    if (res.success) {
                        const form = $('#itemForm');
                        form[0].reset();
                        form.find('#arg_variable').val(res.data.argVariable);
                        form.find('#arg_describe').val(res.data.argDescribe);
                        bootstrap.Modal.getOrCreateInstance(document.getElementById('itemModal')).show();
                    } else {
                        ehrisAlert.error(res.message);
                    }
                })
                .fail(xhr => {
                    Swal.close();
                    ehrisAlert.handleError(xhr);
                });
        });

        $('#btnSaveItem').on('click', function () {
            const form = $('#itemForm');
            const model = {
                ArgVariable: form.find('#arg_variable').val().trim(),
                ArgDescribe: form.find('#arg_describe').val().trim()
            };

            if (!model.ArgDescribe) {
                ehrisAlert.warning('參數說明不可為空！');
                return;
            }

            Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });

            $.ajax({
                url: urls.Update,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(model),
                success: function (res) {
                    Swal.close();
                    ehrisAlert.handle(res).then(function () {
                        if (res.success) {
                            bootstrap.Modal.getInstance(document.getElementById('itemModal')).hide();
                            self.dt.ajax.reload(null, false);
                        }
                    });
                },
                error: function (xhr) {
                    Swal.close();
                    ehrisAlert.handleError(xhr);
                }
            });
        });

        $('#itemTable').on('click', 'tbody .openDept', function () {
            self.pendingSchedules = {};
            const rowData = self.dt.row($(this).closest('tr')).data();
            self.currentArgVariable = rowData.argVariable;
            self.currentMainArgValue = rowData.argValue;

            $('#dept_arg_variable').val(rowData.argVariable);
            $('#dept_arg_describe').val(rowData.argDescribe);

            self.loadDeptGrid(rowData.argVariable);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('deptModal')).show();
        });

        $('#deptGridTable').on('click', '.openSched', function () {
            self.currentDepNo = $(this).data('depno');
            const depName = $(this).data('depname');
            self.currentAgdValue = $(this).closest('tr').find('.agd-value').val() || '';
            $('#sched_dept_name').text(depName);

            self.loadSchedGrid(self.currentArgVariable, self.currentDepNo);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('schedModal')).show();
        });

        $('#btnAddDeptRow').on('click', function () {
            let existingDeps = [];
            $('#deptGridTable tbody tr').each(function () {
                let depNo = $(this).data('depno');
                if (depNo) existingDeps.push(depNo.toString());
            });

            $('#departmentTreeModal').off('shown.bs.modal').on('shown.bs.modal', function () {
                var $select = $('#departmentSelect');
                if ($select.length > 0) {
                    $('#departmentTreeModal').find('.tree-multiselect').remove();
                    $select.show();

                    $select.find('option').prop('selected', false).removeAttr('selected');
                    existingDeps.forEach(val => {
                        $select.find('option[value="' + val + '"]').prop('selected', true).attr('selected', 'selected');
                    });

                    $select.treeMultiselect({
                        searchable: true,
                        searchParams: ['section', 'text'],
                        allowBatchSelection: true,
                        startCollapsed: false,
                        sectionDelimiter: "/"
                    });
                }
            });

            $(document).on('click', '#saveDepartments', function () {
                if ($('#deptModal').hasClass('show')) {
                    const treeModalEl = document.getElementById('departmentTreeModal');
                    const treeModalInstance = bootstrap.Modal.getInstance(treeModalEl);
                    if (treeModalInstance) {
                        treeModalInstance.hide();
                    }
                }
            });

            const treeModal = bootstrap.Modal.getOrCreateInstance(document.getElementById('departmentTreeModal'));
            treeModal.show();
        });

        $('#departmentTreeModal').on('hidden.bs.modal', function () {
            if (!$('#deptModal').hasClass('show')) return;

            const $select = $('#departmentSelect');
            const selectedValues = $select.val() || [];
            const tbody = $('#deptGridTable tbody');

            let currentDeps = {};
            tbody.find('tr').each(function () {
                let tr = $(this);
                let depNo = tr.data('depno');
                let agdNo = tr.data('agdno');
                currentDeps[depNo] = { agdNo: agdNo, tr: tr };
            });

            selectedValues.forEach(val => {
                if (!currentDeps[val]) {
                    let label = $select.find(`option[value="${val}"]`).text() || '';
                    let item = {
                        agdNo: 0,
                        depNo: parseInt(val),
                        depName: label,
                        argValue: self.currentMainArgValue,
                        agdValue: ''
                    };
                    tbody.append(self.createDeptRowHtml(item));
                }
            });

            Object.keys(currentDeps).forEach(depNo => {
                if (!selectedValues.includes(depNo.toString()) && currentDeps[depNo].agdNo === 0) {
                    currentDeps[depNo].tr.remove();
                }
            });

            $('body').addClass('modal-open');
        });

        $('#btnAddSchedRow').on('click', function () {
            const tbody = $('#schedGridTable tbody');
            const item = {
                agsNo: 0,
                agsStartTime: null,
                agsEndTime: null
            };
            tbody.append(self.createSchedRowHtml(item));
        });

        $('#btnSaveAllDept').off('click').on('click', async function () {
            let isValid = true;
            const deptsToSave = [];
            let schedsToSave = [];

            $('#deptGridTable tbody tr').each(function () {
                const tr = $(this);
                const agdNo = parseInt(tr.data('agdno')) || 0;
                let selectedDepNo = parseInt(tr.data('depno')) || 0;
                let agdValue = tr.find('.agd-value').val().trim();

                if (!agdValue) {
                    isValid = false;
                    return false;
                }

                deptsToSave.push({
                    AgdNo: agdNo,
                    ArgVariable: self.currentArgVariable,
                    DepNo: selectedDepNo,
                    AgdValue: agdValue
                });
            });

            if (!isValid) {
                ehrisAlert.warning('單位參數值不可為空');
                return;
            }

            Object.keys(self.pendingSchedules).forEach(depNo => {
                self.pendingSchedules[depNo].forEach(sched => {
                    const currentDeptRow = deptsToSave.find(d => d.DepNo == depNo);
                    if (currentDeptRow) {
                        sched.agsValue = currentDeptRow.AgdValue;
                    }

                    schedsToSave.push({
                        AgsNo: sched.agsNo,
                        ArgVariable: sched.argVariable,
                        DepNo: sched.depNo,
                        AgsValue: sched.agsValue,
                        AgsStartTime: sched.agsStartTime,
                        AgsEndTime: sched.agsEndTime
                    });
                });
            });

            Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });

            $.ajax({
                url: urls.UpdateAllDept,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify({
                    Depts: deptsToSave,
                    Scheds: schedsToSave
                }),
                success: function (res) {
                    Swal.close();
                    if (res.success) {
                        ehrisAlert.success('儲存成功').then(function () {
                            bootstrap.Modal.getInstance(document.getElementById('deptModal')).hide();
                            self.dt.ajax.reload(null, false);
                        });
                    } else {
                        ehrisAlert.error(res.message);
                    }
                },
                error: function (xhr) {
                    Swal.close();
                    ehrisAlert.handleError(xhr);
                }
            });
        });

        $('#btnSaveAllSched').off('click').on('click', function () {
            const timeIntervals = [];
            let isOverlap = false;
            let isTimeValid = true;
            let tempScheds = [];

            $('#schedGridTable tbody tr').each(function () {
                const tr = $(this);
                const startVal = tr.find('.start-time').val();
                const endVal = tr.find('.end-time').val();

                if (!startVal || !endVal) {
                    isTimeValid = false;
                    return false;
                }
                const start = new Date(startVal);
                const end = new Date(endVal);
                if (start >= end) {
                    isTimeValid = false;
                    return false;
                }
                timeIntervals.push({ start: start, end: end });

                tempScheds.push({
                    agsNo: parseInt(tr.data('agsno')) || 0,
                    argVariable: self.currentArgVariable,
                    depNo: self.currentDepNo,
                    agsValue: self.currentAgdValue,
                    agsStartTime: startVal,
                    agsEndTime: endVal
                });
            });

            if (!isTimeValid) {
                ehrisAlert.warning('請填寫完整的開始與結束時間，且結束時間必須大於開始時間');
                return;
            }

            for (let i = 0; i < timeIntervals.length; i++) {
                for (let j = i + 1; j < timeIntervals.length; j++) {
                    if (timeIntervals[i].start < timeIntervals[j].end && timeIntervals[i].end > timeIntervals[j].start) {
                        isOverlap = true;
                        break;
                    }
                }
                if (isOverlap) break;
            }

            if (isOverlap) {
                Swal.fire({ icon: 'error', title: '時間區間重疊', text: '排程設定時間不可重疊！' });
                return;
            }

            self.pendingSchedules[self.currentDepNo] = tempScheds;
            bootstrap.Modal.getInstance(document.getElementById('schedModal')).hide();
        });

        $('#deptGridTable').on('click', '.btnDelDeptRow', function () {
            const tr = $(this).closest('tr');
            const agdNo = parseInt(tr.data('agdno')) || 0;
            const name = $(this).closest('tr').find('.dep-name').val() || '';

            if (agdNo === 0) {
                tr.remove();
                return;
            }

            Swal.fire({
                title: `確定要刪除【${name}】？`,
                text: `刪除後將無法復原！`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: '確定',
                cancelButtonText: '取消'
            }).then(result => {
                if (result.isConfirmed) {
                    $.ajax({
                        url: urls.DeleteDept,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ AgdNo: agdNo }),
                        success: function (res) {
                            if (res.success) {
                                self.loadDeptGrid(self.currentArgVariable);
                                self.dt.ajax.reload(null, false);
                            }
                        }
                    });
                }
            });
        });

        $('#schedGridTable').on('click', '.btnDelSchedRow', function () {
            const tr = $(this).closest('tr');
            const agsNo = parseInt(tr.data('agsno')) || 0;

            if (agsNo === 0) {
                tr.remove();
                return;
            }

            Swal.fire({
                title: '確定要刪除此時間排程？',
                text: `刪除後將無法復原！`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: '確定',
                cancelButtonText: '取消'
            }).then(result => {
                if (result.isConfirmed) {
                    $.ajax({
                        url: urls.DeleteSched,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ AgsNo: agsNo }),
                        success: function (res) {
                            if (res.success) {
                                self.loadSchedGrid(self.currentArgVariable, self.currentDepNo);
                            }
                        }
                    });
                }
            });
        });
    }
};