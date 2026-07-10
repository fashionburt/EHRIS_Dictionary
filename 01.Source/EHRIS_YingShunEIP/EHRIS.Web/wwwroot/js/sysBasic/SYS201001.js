window.SYS201001 = {
    dt: null,
    urls: null,
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
        console.log("SYS201001 初始化完成");
        const self = this;
        const urls = window.adminActions;
        self.urls = urls;

        const $adminModal = $('#adminModal');

        $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
            if (!options.crossDomain && urls.tokenValue) {
                jqXHR.setRequestHeader('RequestVerificationToken', urls.tokenValue);
            }
        });

        const searchableCols = [
            { index: 0, label: '單位' },
            { index: 1, label: '姓名' },
            { index: 2, label: '登入帳號' }
        ];

        $('#birthDate').datepicker();

        $('#ddlUnit').select2({
            language: "zh-TW",
            width: '100%',
            dropdownParent: $adminModal,
            placeholder: "請選擇單位",
            allowClear: false
        });

        $('#ddlProfess').select2({
            language: "zh-TW",
            width: '100%',
            dropdownParent: $adminModal,
            placeholder: "請選擇職稱",
            allowClear: false
        });

        self.dt = createEhrisTable('adminTable', {
            ajaxUrl: urls.getAllAdminsUrl,
            searchableCols: searchableCols,
            ordering: true,  
            order: [[0, 'asc']],  
            columns: [
                { data: 'unit', className: 'dt-col-mid' },
                { data: 'name', className: 'dt-col-mid' },
                { data: 'loginAccount', className: 'dt-col-mid' },
                { data: 'accountStatus', className: 'text-center dt-col-min' },
                { data: 'editAction', className: 'text-center dt-col-action', orderable: false, searchable: false },
                { data: 'deleteAction', className: 'text-center dt-col-action', orderable: false, searchable: false }
            ],
            onInitComplete: function (api) {
                if (urls.hasInsertPermission) {
                    const $wrapper = $(api.table().container());
                    const $rightTop = $wrapper.find('.row:first .col-md-6:last');
                    const $btnGroup = $(`
                        <div class="d-flex justify-content-end align-items-center gap-2">
                            <button id="btnAddAdmin" class="btn btn-success btn-sm" data-bs-toggle="modal" data-bs-target="#adminModal">
                                新增管理員
                            </button>
                        </div>
                    `);
                    $rightTop.append($btnGroup);
                }
                self.dt.columns.adjust().draw(false);
            }
        });

        function adjustTable() {
            if (self.dt) self.dt.columns.adjust().draw(false);
        }

        $(document).on('expanded.lte.sidebar collapsed.lte.sidebar', function () {
            setTimeout(adjustTable, 350);
        });

        $('#adminModal').on('shown.bs.modal', adjustTable);

        $('#adminModal').on('show.bs.modal', function (event) {
            var button = event.relatedTarget;
            if (button && $(button).attr('id') === 'btnAddAdmin') {
                $('.field-validation').text('');
                $('#adminForm label .text-danger').show();
                $('#adminForm')[0].reset();
                $('#adminModalLabel').text('新增管理員');
                $('#modal-mode-indicator').addClass('text-danger').text('(*為必填寫欄位)');

                $('#birthDate').val('');
                $('#adm_email').val('');
                $('#adm_email_bse_no').val(0);
                $('#adm_acct').prop('readonly', false);
                $('#idNumber').prop('readonly', false);
                $('#adm_passwd').closest('.col-md-6').show();
                $('label[for="adm_passwd"]').html('登入密碼 <span class="text-danger">(新增時必填)</span>');
                $('#btnSaveAdmin').data('id', null);
                $('#acc_no').val(0);

                $('#ddlUnit').val('').trigger('change');
                $('#ddlProfess').val('').trigger('change');
                $('input[name="sex"][value="false"]').prop('checked', true);

                loadAllRoles([]);
                loadPersonnelTypes([]);
                $('#manageAllDeptsSwitch').prop('checked', true).trigger('change');
                $('#selectedDepartments').val('');
                $('#selectedDepartmentsTags').empty();
            }
        });

        $('#btnSaveAdmin').on('click', function () {
            const id = $(this).data('id');
            $('.field-validation').text('');
            let isValid = true;
            let errMsg = "";

            let birthDateRoc = $('#birthDate').val().trim();
            let birthDateAd = '';
            if (birthDateRoc) {
                let parts = birthDateRoc.split(/[-/]/);
                if (parts.length === 3 && !isNaN(parts[0]) && parseInt(parts[0], 10) > 0) {
                    let year = parseInt(parts[0], 10) + 1911;
                    birthDateAd = `${year}-${String(parts[1]).padStart(2, '0')}-${String(parts[2]).padStart(2, '0')}`;
                }
            }

            const body = {
                acc_no: parseInt($('#acc_no').val() || 0),
                unitId: parseInt($('#ddlUnit').val() || 0),
                proNo: parseInt($('#ddlProfess').val() || 0),
                name: $('#adm_name').val().trim(),
                idNumber: $('#idNumber').val().trim(),
                birthDate: birthDateAd,
                sex: $('input[name="sex"]:checked').val() === 'true',
                loginAccount: $('#adm_acct').val().trim(),
                password: $('#adm_passwd').val(),
                SelectedPtypeNos: $('#personnelTypeContainer input:checked').map((_, el) => parseInt($(el).val())).get(),
                selectedRoleIds: $('#selectedRoles option').map((_, el) => parseInt($(el).val())).get(),
                managesAllDepts: $('#manageAllDeptsSwitch').is(':checked'),
                selectedDeptIds: ($('#selectedDepartments').val() || '').split(',').filter(id => id).map(Number),
                Emails: []
            };

            const emailValue = $('#adm_email').val().trim();
            if (emailValue) {
                body.Emails.push({
                    BseNo: parseInt($('#adm_email_bse_no').val() || 0),
                    BseEmailType: 'EMAI01',
                    BseEmail: emailValue,
                    BseOrder: 1
                });
            }

            if (emailValue && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(emailValue)) {
                errMsg += "電子郵件格式不正確。\n"; isValid = false;
            }
            if (!body.unitId) { errMsg += "單位為必填項。\n"; isValid = false; }
            if (!body.proNo) { errMsg += "職稱為必填項。\n"; isValid = false; }
            if (!body.name) { errMsg += "姓名為必填項。\n"; isValid = false; }
            if (!body.idNumber || !isValidTaiwanId(body.idNumber)) {
                errMsg += "身分證字號格式不正確。\n"; isValid = false;
            }
            if (!birthDateRoc || !body.birthDate) {
                errMsg += "出生日期不正確。\n"; isValid = false;
            }
            if (!body.loginAccount) { errMsg += "登入帳號為必填項。\n"; isValid = false; }
            if (body.acc_no === 0 && !body.password) {
                errMsg += "新增時，密碼為必填欄位。\n"; isValid = false;
            }

            if (!isValid) {
                Swal.fire({ icon: 'warning', title: '輸入錯誤', html: errMsg.replace(/\n/g, '<br>') });
                return;
            }

            Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
            const url = body.acc_no ? urls.updateAdminUrl : urls.addAdminUrl;

            $.ajax({
                url: url,
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(body),
                success: function (res) {
                    Swal.close();
                    ehrisAlert.handle(res).then(function () {
                        if (res.success) {
                            $('#adminModal').modal('hide');
                            loadMvc(urls.pageUrl);
                        }
                    });
                },
                error: function (xhr) {
                    Swal.close();
                    ehrisAlert.handleError(xhr);
                }
            });
        });

        $('#adminTable').on('click', '.editAdmin', function () {
            const id = $(this).data('id');
            $('.field-validation').text('');
            Swal.fire({ title: '讀取中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });

            $.get(urls.getAdminDetailsUrl, { accNo: id })
                .done(function (res) {
                    Swal.close();
                    if (res.success) {
                        $('#adminForm label .text-danger').hide();
                        const admin = res.data;

                        if (admin.emails && admin.emails.length > 0) {
                            $('#adm_email').val(admin.emails[0].bseEmail || '');
                            $('#adm_email_bse_no').val(admin.emails[0].bseNo || 0);
                        } else {
                            $('#adm_email').val(''); $('#adm_email_bse_no').val(0);
                        }
                        $('#adminModalLabel').text('編輯管理員');
                        $('#acc_no').val(id);
                        $('#btnSaveAdmin').data('id', id);
                        $('#ddlUnit').val(admin.unitId).trigger('change');
                        $('#ddlProfess').val(admin.proNo).trigger('change');
                        $('#adm_name').val(admin.name);
                        $('#adm_acct').val(admin.loginAccount).prop('readonly', true);
                        $('#adm_passwd').val('');
                        $('label[for="adm_passwd"]').html('登入密碼 <span class="text-danger">(需變更請輸入新密碼)</span>');
                        $('#idNumber').val(admin.idNumber).prop('readonly', true);

                        if (admin.birthDate) $('#birthDate').val(toRocDate(admin.birthDate));
                        $('input[name="sex"][value="' + (admin.sex ? 'true' : 'false') + '"]').prop('checked', true);

                        loadAllRoles(admin.selectedRoleIds || []);
                        loadPersonnelTypes(admin.selectedPtypeNos || []);
                        $('#manageAllDeptsSwitch').prop('checked', admin.managesAllDepts).trigger('change');
                        updateSelectedDepartmentsTags(admin.selectedDeptIds || []);

                        $('#adminModal').modal('show');
                    } else {
                        ehrisAlert.error(res.message);
                    }
                })
                .fail(xhr => ehrisAlert.handleError(xhr));
        });

        $('#adminTable').on('click', '.deleteAdmin', function () {
            const id = $(this).data('id');
            const name = $(this).closest('tr').find('td:eq(1)').text();

            Swal.fire({
                title: `確定要刪除【${name}】？`,
                text: `刪除後將無法復原！`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: '確定',
                cancelButtonText: '取消'
            }).then((result) => {
                if (result.isConfirmed) {
                    Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });
                    $.ajax({
                        url: urls.deleteAdminUrl,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify({ accNo: id }),
                        success: function (res) {
                            Swal.close();
                            ehrisAlert.handle(res).then(function () {
                                if (res.success) loadMvc(urls.pageUrl);
                            });
                        },
                        error: xhr => ehrisAlert.handleError(xhr)
                    });
                }
            });
        });

        $('#unselectedRoles').on('dblclick', 'option', function () { $(this).appendTo('#selectedRoles'); });
        $('#selectedRoles').on('dblclick', 'option', function () { $(this).appendTo('#unselectedRoles'); });

        $('#manageAllDeptsSwitch').on('change', function () {
            if ($(this).is(':checked')) {
                $('#deptSelectionContainer').slideUp();
                $('#selectedDepartments').val('');
                $('#selectedDepartmentsTags').empty();
            } else {
                $('#deptSelectionContainer').slideDown();
            }
        });

        $('#btnSelectDepts').on('click', function () {
            const selectedIds = ($('#selectedDepartments').val() || '').split(',').map(String).filter(x => x);
            $('#departmentTreeModal .item .option').each(function () {
                const $checkbox = $(this);
                const value = $checkbox.closest('.item').data('value').toString();
                if (selectedIds.includes(value) !== $checkbox.is(':checked')) $checkbox.trigger('click');
            });
            $('#departmentTreeModal').modal('show');
        });

        $(document).on('click', '.remove-tag', function () {
            var removeVal = $(this).data('value');
            var $hiddenInput = $('#selectedDepartments');
            var selected = ($hiddenInput.val() || '').split(',');
            $hiddenInput.val(selected.filter(v => v != removeVal).join(','));
            $(this).closest('.tag').remove();
        });

        window.handleDepartmentSelection = function (selectedNodes) {
            updateSelectedDepartmentsTags(selectedNodes.map(node => node.id));
        };

        function loadPersonnelTypes(selectedIds = []) {
            const container = $('#personnelTypeContainer').empty().html('<span class="text-muted">載入中...</span>');
            $.get(urls.getAllPersonnelTypesUrl).done(function (types) {
                container.empty();
                if (types && types.length) {
                    const selectedStrs = selectedIds.map(String);
                    types.forEach(type => {
                        const div = document.createElement('div');
                        div.className = 'form-check form-check-inline';
                        div.innerHTML = `
                            <input class="form-check-input" type="checkbox" name="personnelType" id="ptype_${type.ptyNo}" value="${type.ptyNo}" ${selectedStrs.includes(String(type.ptyNo)) ? 'checked' : ''}>
                            <label class="form-check-label" for="ptype_${type.ptyNo}">${type.ptyName}</label>
                        `;
                        container[0].appendChild(div);
                    });
                } else {
                    container.html('<span class="text-muted">沒有可控管的人員類別。</span>');
                }
            }).fail(() => ehrisAlert.error('人員類別載入失敗。'));
        }

        function loadAllRoles(selectedIds = []) {
            const $unselected = $('#unselectedRoles').empty();
            const $selected = $('#selectedRoles').empty();
            $.get(urls.getAllRolesUrl).done(function (allRoles) {
                allRoles.forEach(role => {
                    const option = new Option(role.text, role.value);
                    if (selectedIds.map(String).includes(String(role.value))) $selected.append(option);
                    else $unselected.append(option);
                });
            }).fail(() => ehrisAlert.error('角色列表載入失敗。'));
        }

        function updateSelectedDepartmentsTags(selectedIds) {
            const tagContainer = $('#selectedDepartmentsTags').empty();
            $('#selectedDepartments').val(selectedIds.join(','));
            if (selectedIds.length === 0) return;
            const allDepts = window.departmentTreeData || [];
            selectedIds.forEach(id => {
                const dept = allDepts.find(d => String(d.id) === String(id));
                if (dept) {
                    const span = document.createElement('span');
                    span.className = 'tag badge bg-primary me-1 mb-1';
                    span.innerHTML = `${dept.text} <span class="ms-1 remove-tag" data-value="${dept.id}" style="cursor:pointer;">×</span>`;
                    tagContainer[0].appendChild(span);
                }
            });
        }

        function toRocDate(gregorianDateStr) {
            if (!gregorianDateStr) return "";
            const d = new Date(gregorianDateStr);
            if (isNaN(d.getTime())) return "";
            return (d.getFullYear() - 1911) + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0');
        }

        function isValidTaiwanId(id) {
            const regex = /^[A-Z][1289]\d{8}$/;
            if (typeof id !== 'string' || !regex.test(id)) return false;
            const letterMap = { 'A': 10, 'B': 11, 'C': 12, 'D': 13, 'E': 14, 'F': 15, 'G': 16, 'H': 17, 'I': 34, 'J': 18, 'K': 19, 'L': 20, 'M': 21, 'N': 22, 'O': 35, 'P': 23, 'Q': 24, 'R': 25, 'S': 26, 'T': 27, 'U': 28, 'V': 29, 'W': 32, 'X': 30, 'Y': 31, 'Z': 33 };
            const p = letterMap[id[0]];
            let sum = Math.floor(p / 10) + (p % 10) * 9;
            for (let i = 1; i <= 8; i++) sum += parseInt(id.charAt(i), 10) * (8 - i + 1);
            return parseInt(id.charAt(9), 10) === (10 - (sum % 10)) % 10;
        }
    }
};