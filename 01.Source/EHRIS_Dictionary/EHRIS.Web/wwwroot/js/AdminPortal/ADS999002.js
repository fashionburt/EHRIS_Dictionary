window.ADS999002 = {
dt: null,
    dtPeople: null,
    currentAdrNo: 0,
    urls: window.permissionAction,

    getTreeId: function($tr) {
        const classList = $tr.attr('class').split(/\s+/);
        for (let cls of classList) {
            if (cls.startsWith('treegrid-') && !cls.startsWith('treegrid-parent-')) {
                return cls.replace('treegrid-', '');
            }
        }
        return null;
    },

    getParentId: function($tr) {
        const classList = $tr.attr('class').split(/\s+/);
        for (let cls of classList) {
            if (cls.startsWith('treegrid-parent-')) {
                return cls.replace('treegrid-parent-', '');
            }
        }
        return null;
    },

    updateChildren: function($tr, checked) {
        const self = this;
        const trId = self.getTreeId($tr);
        const $children = $('#permissionTableBody').find('tr.treegrid-parent-' + trId);
        $children.each(function() {
            $(this).find('.chk-item, .chk-sub').prop('checked', checked);
            self.updateChildren($(this), checked);
        });
    },

    updateParentRecursive: function($tr) {
        const self = this;
        const classList = $tr.attr('class').split(/\s+/);
        let parentId = null;
        for (let cls of classList) {
            if (cls.startsWith('treegrid-parent-')) {
                parentId = cls.replace('treegrid-parent-', '');
                break;
            }
        }
        if (!parentId) return;

        const $parentTr = $(`.treegrid-${parentId}`);
        const $siblings = $('#permissionTableBody').find('tr.treegrid-parent-' + parentId);
        let anyChecked = false;

        $siblings.each(function() {
            if ($(this).find('.chk-item').prop('checked')) {
                anyChecked = true;
                return false;
            }
        });

        $parentTr.find('.chk-item').prop('checked', anyChecked);
        self.updateParentRecursive($parentTr);
    },

    renderTreeRows: function(parentContainer, items) {
        const self = this;
        items.forEach(function(item) {
            const tr = document.createElement('tr');
            tr.className = `treegrid-${item.functionId} ${item.parentId ? 'treegrid-parent-' + item.parentId : ''}`;
            tr.setAttribute('data-functype', item.parentId === 0 ? 'sys' : 'func');
            tr.setAttribute('data-parentid', item.parentId);

            const td1 = document.createElement('td');
            const chkItem = document.createElement('input');
            chkItem.type = 'checkbox';
            chkItem.className = 'chk-item form-check-input';
            chkItem.checked = item.isChecked;
            if (item.parentId === 0) chkItem.style.display = 'none';
            td1.appendChild(chkItem);

            const td2 = document.createElement('td');
            td2.textContent = item.functionName;

            const createAuthChk = (isChecked, isVisible) => {
                const td = document.createElement('td');
                td.className = 'text-center';
                const chk = document.createElement('input');
                chk.type = 'checkbox';
                chk.className = 'chk-sub form-check-input';
                chk.checked = isChecked;
                if (!isVisible) chk.style.display = 'none';
                td.appendChild(chk);
                return td;
            };

            tr.appendChild(td1);
            tr.appendChild(td2);
            tr.appendChild(createAuthChk(item.hasCreateAuth, item.addStatus === 1));
            tr.appendChild(createAuthChk(item.hasEditAuth, item.editStatus === 1));
            tr.appendChild(createAuthChk(item.hasDeleteAuth, item.delStatus === 1));

            parentContainer.appendChild(tr);
            if (item.children && item.children.length > 0) {
                self.renderTreeRows(parentContainer, item.children);
            }
        });
    },

    init: function() {
        const self = this;
        const urls = self.urls;

        $.ajaxPrefilter(function(options, originalOptions, jqXHR) {
            if (!options.crossDomain && urls.tokenValue) {
                jqXHR.setRequestHeader('RequestVerificationToken', urls.tokenValue);
            }
        });

        const $doc = $(document);
        $doc.off('click', '.showPeople').on('click', '.showPeople', function(e) {
            e.preventDefault();
            const id = $(this).data('id');
            const name = $(this).data('rolname');
            self.currentAdrNo = id;

            $('#showAdminPeopleModalLabel').text(`人員明細 - ${name}`);

            if (self.dtPeople) {
                self.dtPeople.ajax.reload();
            } else {
                self.dtPeople = createEhrisTable('adminPeopleTable', {
                    ajaxUrl: urls.GetPeople,
                    type: 'POST',
                    contentType: 'application/json',
                    extraData: function(d) {
                        return { adrNo: self.currentAdrNo };
                    },
                    columns: [{ data: 'aduDisplayName', className: 'text-center' }]
                });
            }
            $('#showAdminPeopleModal').modal('show');
        });

        $doc.on('click', '#btnAddRole', function() {
            $('#roleModalLabel').text('新增角色');
            $('#roleForm')[0].reset();
            $('#btnSaveRole').removeData('id');
            $('#adrStatus').prop('checked', true);
        });

        $doc.off('click', '.editRole').on('click', '.editRole', function() {
            const id = $(this).data('id');
            $.get(urls.GetRoleByNo, { adrNo: id }, function(res) {
                if (res.success) {
                    $('#roleModalLabel').text('編輯角色');
                    $('#adrRoleName').val(res.data.adrRoleName);
                    $('#adrRoleMemo').val(res.data.adrRoleMemo);
                    $('#adrStatus').prop('checked', res.data.adrStatus === 1);
                    $('#btnSaveRole').data('id', id);
                    $('#roleModal').modal('show');
                }
            });
        });

        $doc.off('click', '.deleteRole').on('click', '.deleteRole', function() {
            const id = $(this).data('id');
            const name = $(this).closest('tr').find('td:eq(0)').text();

            Swal.fire({
                title: `確定要刪除【${name}】？`,
                text: "刪除後將無法復原！",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#5A2323',
                cancelButtonColor: '#A67C52',
                confirmButtonText: '確定',
                cancelButtonText: '取消'
            }).then((result) => {
                if (result.isConfirmed) {
                    Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
                    $.ajax({
                        url: urls.DeleteRole,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ adrNo: id }),
                        success: function(res) {
                            Swal.close();
                            if (res.success) self.dt.ajax.reload(null, false);
                            ehrisAlert.handle(res);
                        },
                        error: function(xhr) {
                            Swal.close();
                            ehrisAlert.handleError(xhr);
                        }
                    });
                }
            });
        });

        $doc.off('click', '.setRole').on('click', '.setRole', function() {
            const id = $(this).data('id');
            const name = $(this).data('rolname');
            self.currentAdrNo = id;
            $.ajax({
                url: urls.GetPermission,
                type: 'POST',
                data: JSON.stringify({ adrNo: id }),
                contentType: 'application/json',
                success: function(data) {
                    const container = document.getElementById("permissionTableBody");
                    container.innerHTML = '';
                    self.renderTreeRows(container, data);
                    $('#permissionTable').treegrid({ initialState: 'expanded', treeColumn: 1 });
                    $('#setPermissionModalLabel').text(` - ${name}`);
                    $('#setPermissionModal').modal('show');
                }
            });
        });

        $('#btnSaveRole').off('click').on('click', function() {
            const id = $(this).data('id');
            const name = $('#adrRoleName').val().trim();
            if (!name) return ehrisAlert.error('角色名稱必填');
            const body = {
                adrNo: id ?? 0,
                adrRoleName: name,
                adrRoleMemo: $('#adrRoleMemo').val().trim(),
                adrStatus: $('#adrStatus').is(':checked') ? 1 : 0
            };
            $.ajax({
                url: id ? urls.UpdateRole : urls.AddRole,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(body),
                success: function(res) {
                    if (res.success) {
                        $('#roleModal').modal('hide');
                        self.dt.ajax.reload();
                    }
                    ehrisAlert.handle(res);
                }
            });
        });

        $('#btnSavePermission').off('click').on('click', function() {
            const permissions = [];
            $('#permissionTableBody tr').each(function() {
                const $tr = $(this);
                const $chks = $tr.find('input.form-check-input');

                const funcId = self.getTreeId($tr);
                if (!funcId) return;

                permissions.push({
                    adrNo: self.currentAdrNo,
                    adfNo: parseInt(funcId),
                    funcType: $tr.data('functype'),
                    parentAdfNo: parseInt($tr.data('parentid') || 0),
                    isChecked: $chks.eq(0).is(':checked'),
                    canCreate: $chks.eq(1).is(':checked'),
                    canEdit: $chks.eq(2).is(':checked'),
                    canDelete: $chks.eq(3).is(':checked')
                });
            });

            const payload = {
                adrNo: self.currentAdrNo,
                rolePermissions: permissions
            };

            $.ajax({
                url: urls.UpdatePermission,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload),
                success: function(res) {
                    if (res.success) {
                        $('#setPermissionModal').modal('hide');
                    }
                    ehrisAlert.handle(res);
                },
                error: function(xhr) {
                    ehrisAlert.handleError(xhr);
                }
            });
        });

        self.dt = createEhrisTable('roleTable', {
            ajaxUrl: urls.GetAllRoleList,
            langUrl: urls.dataTableLangUrl,
            searchableCols: [{ index: 0, label: '角色名稱' }, { index: 1, label: '描述' }],
            columns: [
                { data: 'adrRoleName' },
                { data: 'adrRoleMemo' },
                { data: 'statusText', className: 'text-center' },
                { data: 'modifyTimeText', className: 'text-center' },
                { data: 'peopleAction', className: 'text-center', orderable: false },
                { data: 'authorityAction', className: 'text-center', orderable: false },
                { data: 'editAction', className: 'text-center', orderable: false },
                { data: 'delAction', className: 'text-center', orderable: false }
            ],
            onInitComplete: function(api) {
                if (urls.hasInsertPermission) {
                    const $wrapper = $(api.table().container());
                    const $rightTop = $wrapper.find('.row:first .col-md-6:last');
                    if ($rightTop.find('#btnAddRole').length === 0) {
                        $rightTop.append(`
                <div class="d-flex justify-content-end align-items-center gap-2">
                    <button id="btnAddRole" class="btn btn-success btn-sm" data-bs-toggle="modal" data-bs-target="#roleModal">新增角色</button>
                </div>
            `);
                    }
                }
            }
        });
        $('#permissionTableBody').on('change', '.chk-item', function() {
            const $tr = $(this).closest('tr');
            self.updateChildren($tr, $(this).is(':checked'));
            self.updateParentRecursive($tr);
        });

        $('#permissionTableBody').on('change', '.chk-sub', function() {
            const $tr = $(this).closest('tr');
            if ($(this).is(':checked')) {
                $tr.find('.chk-item').prop('checked', true);
            }
            self.updateParentRecursive($tr);
        });
    }
};