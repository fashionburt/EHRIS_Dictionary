window.ADS999999 = {
    currentLevels: [],

    init: function() {
        const self = this;
        const config = window.ads999999Config;

        $(document).off('click', '#btnManageLevels');
        $(document).off('click', '#btnAddLevelRow');
        $(document).off('click', '.edit-level-btn');
        $(document).off('click', '.delete-level-btn');
        $('#btnConfirmRow').off('click');
        $('#btnFinalSaveLevels').off('click');
        $('#btnSave').off('click');
        $('#btnUploadBg').off('click');
        $('#btnCancel').off('click');

        $(document).on('click', '#btnManageLevels', function() {
            self.loadLevelManagerModal();
        });

        $(document).on('click', '#btnAddLevelRow', function() {
            self.openEditor(-1);
        });

        $(document).on('click', '.edit-level-btn', function() {
            const index = $(this).data('index');
            self.openEditor(index);
        });

        $(document).on('click', '.delete-level-btn', function() {
            const index = $(this).data('index');
            self.removeRow(index);
        });

        $('#btnConfirmRow').on('click', function() {
            self.applyRowChange();
        });

        $('#btnFinalSaveLevels').on('click', function() {
            self.saveLevelsToDb();
        });

        $('#btnSave').on('click', function() {
            const $form = $('#systemInfoForm');
            if (!$form[0].checkValidity()) {
                $form[0].reportValidity();
                return;
            }

            const formData = {
                SyiTitle: $('input[name="SyiTitle"]').val(),
                SyiSubTitle: $('input[name="SyiSubTitle"]').val(),
                SyiSmtpServer: $('input[name="SyiSmtpServer"]').val(),
                SyiSmtpPort: parseInt($('input[name="SyiSmtpPort"]').val() || 0),
                SyiSmtpSsl: $('#SyiSmtpSsl').is(':checked'),
                SyiEmailAddr: $('input[name="SyiEmailAddr"]').val(),
                SyiEmailPwdPlain: $('input[name="SyiEmailPwdPlain"]').val(),
                SyiEmailPwdKeyVersion: parseInt($('input[name="SyiEmailPwdKeyVersion"]').val() || 0),
                SyiMultipleMode: $('#SyiMultipleMode').is(':checked') ? 1 : 0
            };

            Swal.fire({
                title: '確認儲存？',
                text: "系統設定將立即更新，您將會被強制登出並重新載入設定",
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: '確定',
                cancelButtonText: '取消',
                confirmButtonColor: '#A67C52',
                cancelButtonColor: '#E6D5C6'
            }).then((result) => {
                if (result.isConfirmed) {
                    $.ajax({
                        url: config.updateUrl,
                        type: 'POST',
                        contentType: 'application/json',
                        data: JSON.stringify(formData),
                        success: function(res) {
                            if (res.success) {
                                window.location.href = res.redirectUrl;
                            } else {
                                ehrisAlert.handle(res);
                            }
                        },
                        error: function(xhr) {
                            ehrisAlert.handleError(xhr);
                        }
                    });
                }
            });
        });

        $('#btnUploadBg').on('click', function() {
            const fileInput = $('#BackgroundImage')[0];
            if (!fileInput || fileInput.files.length === 0) {
                Swal.fire('請選擇檔案', '', 'warning');
                return;
            }

            const file = fileInput.files[0];
            if (file.size > 20 * 1024 * 1024) {
                Swal.fire('檔案過大', '上限為 20MB', 'error');
                return;
            }

            const formData = new FormData();
            formData.append('file', fileInput.files[0]);

            Swal.fire({
                title: '確認上傳？',
                text: "這將覆蓋現有的背景圖",
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: '確定',
                cancelButtonText: '取消',
                confirmButtonColor: '#A67C52',
                cancelButtonColor: '#E6D5C6'
            }).then((result) => {
                if (result.isConfirmed) {
                    $.ajax({
                        url: config.uploadBgUrl,
                        type: 'POST',
                        data: formData,
                        processData: false,
                        contentType: false,
                        success: function(res) {
                            if (res.success) {
                                Swal.fire('成功', res.message, 'success').then(() => {
                                    $('.nav-link.active').click();
                                });
                            } else {
                                ehrisAlert.handle(res);
                            }
                        },
                        error: function(xhr) {
                            ehrisAlert.handleError(xhr);
                        }
                    });
                }
            });
        });

        $('#btnCancel').on('click', function() {
            $('.nav-link.active').click();
        });
    },

    loadLevelManagerModal: function() {
        const self = this;
        $('#levelManagerModal').modal('show');
        $('#levelManagerContent').html('<div class="text-center p-3"><div class="spinner-border spinner-border-sm"></div> 讀取中...</div>');

        $.get('/ADS999999/GetLevelData', function(html) {
            $('#levelManagerContent').html(html);
            const rawJson = $('#originDataJson').val();
            self.currentLevels = rawJson ? JSON.parse(rawJson) : [];
            self.renderTable();
        });
    },

    renderTable: function() {
        const self = this;
        const $body = $('#dynamicLevelBody');
        $body.empty();

        const activeItems = self.currentLevels.filter(x => x.Status !== 2);

        if (activeItems.length === 0) {
            $body.append('<tr><td colspan="5" class="text-center text-muted">目前暫無層級資料</td></tr>');
            return;
        }

        activeItems.forEach((item) => {
            const realIndex = self.currentLevels.indexOf(item);
            const statusBadge = item.Status === 1
                ? '<span class="badge badge-enabled">啟用</span>'
                : '<span class="badge badge-disabled">關閉</span>';

            const tr = `
                <tr>
                    <td class="text-center">${item.No}</td>
                    <td class="text-center">${item.Name}</td>
                    <td class="text-center">${item.Rank}</td>
                    <td class="text-center">${statusBadge}</td>
                    <td class="text-center">
                        <button type="button" class="icon-btn edit-level-btn text-primary" data-index="${realIndex}">
                            <i class="fa-solid fa-pen-to-square"></i>
                        </button>
                    </td>
                    <td class="text-center">
                        <button type="button" class="icon-btn delete-level-btn text-danger" data-index="${realIndex}">
                            <i class="fa-solid fa-trash"></i>
                        </button>
                    </td>                </tr>`;
            $body.append(tr);
        });
    },

    openEditor: function(index) {
        const self = this;
        if (index === -1) {
            $('#editModalLabel').text('新增權限層級');
            $('#tempIndex').val(-1);
            $('#inpName').val('');
            $('#inpRank').val(1);
            $('#inpStatus').val(1);
        } else {
            const data = self.currentLevels[index];
            $('#editModalLabel').text('修改權限層級');
            $('#tempIndex').val(index);
            $('#inpName').val(data.Name);
            $('#inpRank').val(data.Rank);
            $('#inpStatus').val(data.Status);
        }
        $('#levelEditModal').modal('show');
    },

    applyRowChange: function() {
        const self = this;
        const index = parseInt($('#tempIndex').val());
        const rowData = {
            Name: $('#inpName').val().trim(),
            Rank: parseInt($('#inpRank').val() || 0),
            Status: parseInt($('#inpStatus').val())
        };

        if (!rowData.Name) { ehrisAlert.warning('請輸入層級名稱'); return; }

        if (!rowData.Rank) { ehrisAlert.warning('請輸入權限值'); return; }

        if (index === -1) {
            const maxNo = self.currentLevels.length > 0 ? Math.max(...self.currentLevels.map(o => o.No)) : 0;
            rowData.No = maxNo + 1;
            self.currentLevels.push(rowData);
        } else {
            rowData.No = self.currentLevels[index].No;
            self.currentLevels[index] = rowData;
        }

        $('#levelEditModal').modal('hide');
        self.renderTable();
    },

    removeRow: function (index) {
        const self = this;
        Swal.fire({
            title: `確定要刪除？`,
            text: `刪除後將無法復原！`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#A67C52',
            cancelButtonColor: '#E6D5C6',
            confirmButtonText: '確定',
            cancelButtonText: '取消'
        }).then((result) => {
            if (result.isConfirmed) {
                self.currentLevels[index].Status = 2;
                self.renderTable();
            }
        });
    },

    saveLevelsToDb: function() {
        const self = this;
        Swal.fire({
            title: '確認儲存所有權限設定？',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#A67C52',
            cancelButtonColor: '#E6D5C6',
            confirmButtonText: '儲存',
            cancelButtonText: '取消'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/ADS999999/UpdateLevelData',
                    type: 'POST',
                    contentType: 'application/json',
                    data: JSON.stringify({ Levels: self.currentLevels }),
                    success: function(res) {
                        ehrisAlert.handle(res).then(() => {
                            if (res.success) $('#levelManagerModal').modal('hide');
                        });
                    }
                });
            }
        });
    }
};

$(function() {
    ADS999999.init();
});