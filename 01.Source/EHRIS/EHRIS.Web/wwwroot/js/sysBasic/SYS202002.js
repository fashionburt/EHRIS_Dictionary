window.SYS202002 = {
    table: null,
    action: null,

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
        console.log("SYS202002 初始化完成");
        const self = this;
        const action = window.permissionAction || {};
        self.action = action;
        const $table = $('#functionTable');
        self.table = $table;

        $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
            if (!options.crossDomain && action.tokenValue) {
                jqXHR.setRequestHeader('RequestVerificationToken', action.tokenValue);
            }
        });

        function permissionFormatter(value) {
            return value === 1 ? '<i class="fa fa-check text-success"></i>' : '<i class="fa fa-times text-danger"></i>';
        }

        function statusFormatter(value) {
            const status = (value !== null && value !== undefined) ? value.toString().trim() : '';
            if (status === "1") { return '<span class="badge bg-success">上架</span>'; }
            else if (status === "0") { return '<span class="badge bg-danger">下架</span>'; }
            else { return '<span class="badge bg-secondary">未知</span>'; }
        }

        function initTreegrid() {
            const sfuNameColumnIndex = self.table.bootstrapTable('getVisibleColumns').findIndex(col => col.field === 'sfuName');
            if (sfuNameColumnIndex > -1) {
                self.table.treegrid({
                    treeColumn: sfuNameColumnIndex,
                    initialState: 'expanded',
                    expanderExpandedClass: 'bi bi-caret-down-fill',
                    expanderCollapsedClass: 'bi bi-caret-right-fill',
                    onChange: function () {
                        self.table.bootstrapTable('resetView');
                    }
                });
            }
        }

        function loadTableData() {
            if (!action.getDataForTree) return ehrisAlert.error('系統錯誤：GetDataForTree URL 未定義。');
            $.get(action.getDataForTree, function (data) {
                const validIds = new Set(data.map(item => item.id));
                data.forEach(item => {
                    item.id = Number(item.id);
                    item.pid = (item.pid === 0 || item.pid === null) ? null : Number(item.pid);
                    if (item.pid !== null && !validIds.has(item.pid)) {
                        item.pid = null;
                    }
                });
                initializeTable(data);
            }).fail((xhr) => {
                ehrisAlert.handleError(xhr);
            });
        }

        function customTreeSearch(data, text) {
            if (!text) return data;
            const searchText = text.toLowerCase();
            const keepIds = new Set();
            const dataMap = new Map();
            data.forEach(row => dataMap.set(row.id, row));
            data.forEach(row => {
                const matchesId = row.id.toString().toLowerCase().includes(searchText);
                const matchesSysName = row.sysName && row.sysName.toLowerCase().includes(searchText);
                const matchesSfuName = row.sfuName && row.sfuName.toLowerCase().includes(searchText);
                if (matchesId || matchesSysName || matchesSfuName) {
                    keepIds.add(row.id);
                    let currentPid = row.pid;
                    while (currentPid !== null && currentPid !== 0) {
                        if (keepIds.has(currentPid)) break;
                        keepIds.add(currentPid);
                        const parentRow = dataMap.get(currentPid);
                        if (parentRow) currentPid = parentRow.pid;
                        else break;
                    }
                }
            });
            return data.filter(row => keepIds.has(row.id));
        }

        function initializeTable(tableData) {
            self.table.bootstrapTable('destroy').bootstrapTable({
                data: tableData,
                toolbar: '#toolbar',
                toolbarAlign: 'right',
                idField: 'id',
                parentIdField: 'pid',
                treeShowField: 'sfuName',
                search: true,
                searchAlign: 'left',
                customSearch: customTreeSearch,
                formatSearch: function () { return '請輸入關鍵字'; },
                showColumns: true,
                showRefresh: true,
                onRefresh: function () { loadTableData(); },
                buttonsAlign: 'right',
                classes: 'table table-bordered table-hover table-sm',
                locale: 'zh-TW',
                iconSize: 'sm',
                maintainMetaData: true,
                columns: [
                    { field: 'id', title: '系統編號', align: 'center', width: '10%', sortable: true },
                    { field: 'sysName', title: '分類名稱', align: 'center', width: '10%', sortable: true },
                    { field: 'sfuName', title: '功能名稱', switchable: false },
                    { field: 'sfuIns', title: '新增', align: 'center', width: '5%', formatter: permissionFormatter, sortable: true },
                    { field: 'sfuEdi', title: '編輯', align: 'center', width: '5%', formatter: permissionFormatter, sortable: true },
                    { field: 'sfuDel', title: '刪除', align: 'center', width: '5%', formatter: permissionFormatter, sortable: true },
                    { field: 'sfuStatus', title: '狀態', align: 'center', width: '8%', formatter: statusFormatter },
                    { field: 'editAction', title: '修改', align: 'center', width: '7%', escape: false },
                    { field: 'deleteAction', title: '刪除', align: 'center', width: '7%', escape: false }
                ],
                onPostBody: initTreegrid,
                onToggle: initTreegrid
            });
        }

        function populateSystemsDropdown(selectedId) {
            if (!action.getSystems) return $.Deferred().reject();
            const $select = $('#sysNo');
            return $.get(action.getSystems, function (systems) {
                $select.html('<option value="" disabled selected hidden>請選擇系統</option>');
                systems.forEach(sys => $select.append(new Option(sys.text, sys.value)));
                if (selectedId) $select.val(selectedId);
            }).fail(function (xhr) {
                ehrisAlert.handleError(xhr);
            });
        }

        function populateParentsDropdown(sysNo, selectedParentId) {
            if (!action.getFunctionsBySystem) return $.Deferred().resolve();
            const $select = $('#sfuParent').html('<option value="0">無 (頂層功能)</option>').prop('disabled', true);
            if (sysNo) {
                return $.get(action.getFunctionsBySystem, { sysNo }, function (functions) {
                    functions.forEach(func => $select.append(new Option(func.text, func.value)));
                    if (selectedParentId !== undefined && selectedParentId !== null) $select.val(selectedParentId);
                    $select.prop('disabled', false);
                }).fail(function (xhr) {
                    ehrisAlert.handleError(xhr);
                });
            }
            return $.Deferred().resolve();
        }

        $('#sysNo').on('change', function () {
            populateParentsDropdown($(this).val());
        });

        $('#functionModal').on('show.bs.modal', function (event) {
            var button = event.relatedTarget;
            var isAddMode = $(button).is('#btnAddFunction');
            if (isAddMode) {
                $('#btnSaveFunction').data('id', null);
                $('#functionModalLabel').text('新增功能');
                $('#functionModal .required-asterisk').show();
                $('#modal-mode-indicator').addClass('text-danger').text('(*為必填寫欄位)');
                $('#sfuNo').prop('readonly', false);
                $('#sfuIns, #sfuEdi, #sfuDel').prop('checked', true);
                populateSystemsDropdown();
                $('#sfuParent').html('<option value="0">無 (頂層功能)</option>');
            }
        });

        self.table.on('click', '.editFunction', function () {
            if (!action.getFunctionDetails) return ehrisAlert.error('系統錯誤：GetFunctionDetails URL 未定義。');
            const id = $(this).data('id');
            Swal.fire({ title: '讀取中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
            $.get(action.getFunctionDetails, { id: id }, function (res) {
                Swal.close();
                if (res.success === false) return ehrisAlert.error(res.message);
                $('#functionForm')[0].reset();
                $('#functionForm').removeClass('was-validated');
                $('#functionModalLabel').text('編輯功能');
                $('#modal-mode-indicator').removeClass('text-danger').text('(編輯模式)');
                $('#functionModal .required-asterisk').hide();
                $('#btnSaveFunction').data('id', id);
                $('#sfuNo').val(res.sfuNo).prop('readonly', true);
                $('#sfuName').val(res.sfuName);
                $('#sfuCatalog').val(res.sfuCatalog);
                $('#sfuPath').val(res.sfuPath);
                $('#sfuOrder').val(res.sfuOrder);

                const statusStr = (res.sfuStatus !== null && res.sfuStatus !== undefined) ? res.sfuStatus.toString().trim() : '';
                $(`input[name="sfuStatus"][value="${statusStr}"]`).prop('checked', true);

                $('#sfuIns').prop('checked', res.sfuIns === 1);
                $('#sfuEdi').prop('checked', res.sfuEdi === 1);
                $('#sfuDel').prop('checked', res.sfuDel === 1);
                populateSystemsDropdown(res.sysNo).done(() => {
                    populateParentsDropdown(res.sysNo, res.sfuParent);
                });
                $('#functionModal').modal('show');
            }).fail((xhr) => {
                Swal.close();
                ehrisAlert.handleError(xhr);
            });
        });

        $('#btnSaveFunction').on('click', function () {
            const form = $('#functionForm')[0];
            if (!form.checkValidity()) {
                form.classList.add('was-validated');
                return;
            }
            const id = $(this).data('id');
            const isUpdate = id !== null;
            const body = {
                SfuNo: parseInt($('#sfuNo').val()),
                SfuName: $('#sfuName').val(),
                SysNo: parseInt($('#sysNo').val()),
                SfuParent: $('#sfuParent').val() ? parseInt($('#sfuParent').val()) : 0,
                SfuCatalog: $('#sfuCatalog').val(),
                SfuPath: $('#sfuPath').val(),
                SfuOrder: parseInt($('#sfuOrder').val()),
                SfuStatus: $('input[name="sfuStatus"]:checked').val(),
                SfuIns: $('#sfuIns').is(':checked') ? 1 : 0,
                SfuEdi: $('#sfuEdi').is(':checked') ? 1 : 0,
                SfuDel: $('#sfuDel').is(':checked') ? 1 : 0
            };
            const url = isUpdate ? action.updateFunction : action.createFunction;
            if (!url) return ehrisAlert.error(`系統錯誤：${isUpdate ? 'UpdateFunction' : 'CreateFunction'} URL 未定義。`);

            Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
            $.ajax({
                url: url,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(body),
                success: function (res) {
                    Swal.close();
                    ehrisAlert.handle(res).then(function () {
                        if (res.success) {
                            $('#functionModal').modal('hide');
                            loadMvc(action.pageUrl);
                        }
                    });
                },
                error: function (xhr) {
                    Swal.close();
                    ehrisAlert.handleError(xhr);
                }
            });
        });

        self.table.on('click', '.deleteFunction', function () {
            if (!action.deleteFunction) return ehrisAlert.error('系統錯誤：DeleteFunction URL 未定義。');
            const button = $(this);
            const id = button.data('id');
            const row = button.closest('tr');
            const nameCell = row.find('td[data-field="sfuName"]');
            const name = $(this).closest('tr').find('td:eq(2)').text();

            Swal.fire({
                title: `確定要刪除【${name}】？`,
                text: "其下的所有子功能也將一併刪除！",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: '確定',
                cancelButtonText: '取消'
            }).then((result) => {
                if (result.isConfirmed) {
                    Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
                    $.ajax({
                        url: action.deleteFunction,
                        type: 'POST',
                        data: { id: id },
                        success: function (res) {
                            Swal.close();
                            ehrisAlert.handle(res).then(function () {
                                if (res.success) loadMvc(action.pageUrl);
                            });
                        },
                        error: function (xhr) {
                            Swal.close();
                            ehrisAlert.handleError(xhr);
                        }
                    });
                }
            });
        });

        $('#functionModal').on('hidden.bs.modal', function () {
            $('#functionForm')[0].reset();
            $('#functionForm').removeClass('was-validated');
        });

        loadTableData();
    }
};