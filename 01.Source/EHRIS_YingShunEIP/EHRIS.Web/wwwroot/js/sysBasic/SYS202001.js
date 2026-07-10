window.SYS202001 = {
    dt: null,
    subDt: null,
    urls: null,
    modalHistory: [],
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
        const urls = window.permissionAction;
        self.urls = urls;

        $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
            if (!options.crossDomain && urls.tokenValue) {
                jqXHR.setRequestHeader('RequestVerificationToken', urls.tokenValue);
            }
        });

        const searchableCols = [
            { index: 0, label: '部門代碼' },
            { index: 1, label: '部門名稱' },
            { index: 3, label: '修建者' }
        ];

        const commonColumns = [
            { data: 'deptCode', className: 'dt-col-mid' },
            { data: 'deptName', className: 'dt-col-max' },
            { data: 'deptOrder', className: 'text-center dt-col-order' },
            { data: 'deptModifyName', className: 'dt-col-mod', orderable: false },
            {
                data: 'deptModifyTime',
                className: 'dt-col-time',
                render: function (data, type) {
                    if (type === 'display' && data && data.length > 16) {
                        return data.substring(0, 16);
                    }
                    return data;
                }
            },
            { data: 'editAction', className: 'text-center dt-col-action', orderable: false, searchable: false },
            { data: 'deleteAction', className: 'text-center dt-col-action', orderable: false, searchable: false }
        ];

        self.dt = createEhrisTable('departmentTable', {
            ajaxUrl: urls.getDepartmentsUrl,
            langUrl: urls.dataTableLangUrl,
            searchableCols: searchableCols,
            columns: commonColumns,
            onInitComplete: function (api) {
                if (urls.hasInsertPermission) {
                    const $wrapper = $(api.table().container());
                    const $rightTop = $wrapper.find('.row:first .col-md-6:last');
                    const $btnGroup = $(`
                        <div class="d-flex justify-content-end align-items-center gap-2">
                            <button id="btnAdd" type="button" class="btn btn-success btn-sm" style="min-width:85px !important;">
                                新增部門
                            </button>
                        </div>
                    `);
                    $rightTop.append($btnGroup);
                }
                self.dt.columns.adjust().draw(false);
            }
        });

        function loadSubDepartmentTable(parentId) {
            if (self.subDt) { self.subDt.destroy(); self.subDt = null; }

            self.subDt = createEhrisTable('subDepartmentTable', {
                ajaxUrl: urls.getSubDepartmentsUrl + '?parentId=' + parentId,
                searchableCols: searchableCols,
                columns: commonColumns,
                onInitComplete: function (api) {
                    const canInsert = $('#hdnCanInsertSub').val() === 'true';
                    const level = parseInt($('#hdnDeptLevel').val() || 0);
                    const MAX_LEVEL = 100;

                    if (canInsert) {
                        const $wrapper = $(api.table().container());
                        const $rightTop = $wrapper.find('.row:first .col-md-6:last');

                        let btnHtml = '';
                        if (level < MAX_LEVEL) {
                            btnHtml = '<button id="btnAddSubDept" class="btn btn-success btn-sm" style="min-width:90px !important;">新增子部門</button>';
                        } else {
                            btnHtml = '<button class="btn btn-secondary btn-sm" disabled title="已達最大層級">新增子部門</button>';
                        }
                        $rightTop.append($(`<div class="d-flex justify-content-end align-items-center gap-2">${btnHtml}</div>`));
                    }

                    api.order([2, 'asc']).draw(false);
                    self.subDt.columns.adjust().draw(false);
                }
            });
        }

        function saveDepartment(url, model, successCallback) {
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

        $('#editModal').on('hidden.bs.modal', function () {
            if (self.subDt) { self.subDt.destroy(); self.subDt = null; }
            $('#editModalBody').empty();
            self.modalHistory = [];
        });

        $('#editModal').on('hide.bs.modal', function (e) {
            if (self.modalHistory.length > 1) {
                e.preventDefault();
                self.modalHistory.pop();
                const parentLevel = self.modalHistory[self.modalHistory.length - 1];
                loadEditModal(parentLevel.id, parentLevel.name);
            }
        });

        function loadEditModal(id, deptName) {
            const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('editModal'));
            const modalBody = $('#editModalBody');
            modalBody.html('<div class="d-flex justify-content-center my-5"><div class="spinner-border text-primary" role="status"></div></div>');
            modal.show();

            $.get(`${urls.getEditPartialUrl}?id=${id}`)
                .done(function (htmlResult) {
                    $('#editModalLabel').text(`編輯部門: ${deptName || ''}`);
                    modalBody.html(htmlResult);
                    loadSubDepartmentTable(id);
                })
                .fail(xhr => ehrisAlert.handleError(xhr));
        }

        $(document).on('click', '#btnAdd', function () {
            const form = $('#departmentForm');
            form[0].reset();
            $('#modalLabel').text('新增頂層部門');
            $('#modal-mode-indicator').addClass('text-danger').text('(*為必填寫欄位)');
            form.find('#deptId').val(0);
            form.find('#parentDeptName').val('無');
            bootstrap.Modal.getOrCreateInstance(document.getElementById('departmentModal')).show();
        });

        $(document).on('click', '#departmentTable tbody .btn-edit', function () {
            const id = $(this).data('id');
            const deptName = $(this).closest('tr').find('td:eq(1)').text();
            self.modalHistory = [{ id: id, name: deptName }];
            loadEditModal(id, deptName);
        });

        $(document).on('click', '#subDepartmentTable tbody .btn-drill-down', function () {
            const id = $(this).data('id');
            const deptName = $(this).closest('tr').find('td:eq(1)').text();
            self.modalHistory.push({ id: id, name: deptName });
            loadEditModal(id, deptName);
        });

        $('#btnSaveNewDept').on('click', function () {
            const form = $('#departmentForm');
            const model = {
                deptId: 0,
                deptCode: form.find('#deptCode').val().trim(),
                deptName: form.find('#deptName').val().trim(),
                deptOrder: parseInt(form.find('#deptOrder').val() || 0),
                parentId: 1
            };
            if (!model.deptCode || !model.deptName) {
                ehrisAlert.warning('請填寫必填欄位');
                return;
            }
            saveDepartment(urls.createUrl, model, () => {
                bootstrap.Modal.getInstance(document.getElementById('departmentModal')).hide();
                self.dt.ajax.reload(null, false);
            });
        });

        $(document).on('click', '#btnSaveEdit', function () {
            const form = $('#editDepartmentForm');
            const model = {
                deptId: parseInt(form.find('#deptId').val()),
                deptCode: form.find('#deptCode').val().trim(),
                deptName: form.find('#deptName').val().trim(),
                deptOrder: parseInt(form.find('#deptOrder').val() || 0),
                parentId: form.data('parent-id') || null
            };
            saveDepartment(urls.updateUrl, model, () => {
                bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
                self.dt.ajax.reload(null, false);
            });
        });

        $(document).on('click', '#btnAddSubDept', function () {
            const parentName = $('#editDepartmentForm #deptName').val();
            const subForm = $('#subDepartmentForm');
            subForm[0].reset();
            $('#subModalLabel').text('新增子部門');
            subForm.find('#subDeptId').val(0);
            subForm.find('#subParentDeptName').val(parentName);
            subForm.find('#subDeptCode').prop('readonly', false);
            $('#btnSuggestCode').show();
            bootstrap.Modal.getOrCreateInstance(document.getElementById('subDepartmentModal')).show();
        });

        $('#btnSaveSubDept').on('click', function () {
            const id = parseInt($('#subDeptId').val() || 0);
            const model = {
                deptId: id,
                deptCode: $('#subDeptCode').val().trim(),
                deptName: $('#subDeptName').val().trim(),
                deptOrder: parseInt($('#subDeptOrder').val() || 0),
                parentId: parseInt($('#editDepartmentForm #deptId').val())
            };
            const url = (id === 0) ? urls.createUrl : urls.updateUrl;
            saveDepartment(url, model, () => {
                bootstrap.Modal.getInstance(document.getElementById('subDepartmentModal')).hide();
                self.dt.ajax.reload(null, false);
                if (self.subDt) self.subDt.ajax.reload(null, false);
            });
        });

        $(document).on('click', '#btnSuggestCode', function () {
            const parentId = $('#editDepartmentForm #deptId').val();
            const $input = $('#subDeptCode');
            $input.prop('disabled', true);
            $.get(urls.suggestCodeUrl, { parentId: parentId })
                .done(res => {
                    if (res.success) {
                        $input.val(res.data).prop('readonly', true);
                    } else {
                        ehrisAlert.warning(res.message || '無法產生建議代碼');
                    }
                })
                .fail(xhr => ehrisAlert.handleError(xhr))
                .always(() => $input.prop('disabled', false));
        });

        const handleDelete = (id, name, isSub) => {
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
                        url: urls.deleteUrl,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ id: id }),
                        success: res => ehrisAlert.handle(res).then(() => {
                            self.dt.ajax.reload(null, false);
                            if (isSub && self.subDt) self.subDt.ajax.reload(null, false);
                        }),
                        error: xhr => ehrisAlert.handleError(xhr)
                    });
                }
            });
        };

        $(document).on('click', '#departmentTable .btn-delete', function () {
            handleDelete($(this).data('id'), $(this).closest('tr').find('td:eq(1)').text(), false);
        });

        $(document).on('click', '#subDepartmentTable .btn-delete-sub', function () {
            handleDelete($(this).data('id'), $(this).closest('tr').find('td:eq(1)').text(), true);
        });
    }
};