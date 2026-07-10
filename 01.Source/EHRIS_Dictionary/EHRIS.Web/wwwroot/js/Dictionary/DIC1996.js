window.DIC1996 = {
    dt: null,
    urls: window.permissionAction,

    init: function () {
        const self = this;
        const action = self.urls;

        $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
            if (!options.crossDomain && action.tokenValue) {
                jqXHR.setRequestHeader('RequestVerificationToken', action.tokenValue);
            }
        });

        self.dt = $('#announcementTable').DataTable({
            serverSide: true,
            processing: true,
            searching: false,
            ajax: {
                url: self.urls.getData,
                type: 'POST',
                contentType: 'application/json',
                data: function (d) {
                    return JSON.stringify({
                        draw: d.draw,
                        start: d.start,
                        length: d.length,
                        orderby: d.order,
                        columns: d.columns,
                        extraSearch: {
                            searchValue: $('#txtKeyword').val()
                        }
                    });
                }
            },
            columns: [
                {
                    data: 'priorityText',
                    className: 'text-center',
                    render: function (data, type, row) {
                        let cls = row.priority === 2 ? 'bg-danger' : (row.priority === 1 ? 'bg-warning text-dark' : 'bg-info');
                        return `<span class="badge ${cls}">${data}</span>`;
                    }
                },
                {
                    data: 'message',
                    className: 'text-start',
                    render: function (data) {
                        if (!data) return '';
                        let decoded = $('<div>').html(data).text();
                        const urlRegex = /(https?:\/\/[^\s]+)/g;
                        return decoded.replace(urlRegex, function (url) {
                            let displayUrl = url.length > 30 ? url.substring(0, 27) + "..." : url;
                            return `<a href="${url}" target="_blank" rel="noopener noreferrer" title="${url}">${displayUrl}</a>`;
                        });
                    }
                },
                {
                    data: 'isEnabled',
                    className: 'text-center',
                    render: function (data) {
                        return data ? '<span class="badge bg-success">啟用</span>' : '<span class="badge bg-secondary">停用</span>';
                    }
                },
                { data: 'startDate_Text', className: 'text-center' },
                { data: 'endDate_Text', className: 'text-center' },
                {
                    data: 'editAction',
                    className: 'text-center',
                    orderable: false,
                    render: function (data) {
                        return $('<div>').html(data).text();
                    }
                },
                {
                    data: 'delAction',
                    className: 'text-center',
                    orderable: false,
                    render: function (data) {
                        return $('<div>').html(data).text();
                    }
                }
            ],
            order: [[3, 'desc']],
            language: { url: self.urls.dataTableLangUrl }
        });

        $('#btnSearch').on('click', () => self.dt.ajax.reload());
        $('#txtKeyword').on('keypress', (e) => { if (e.which === 13) self.dt.ajax.reload(); });
    },

    openModal: function () {
        $('#editId').val(0);
        $('#editForm')[0].reset();
        $('#editStartDate').val(new Date().toISOString().slice(0, 16));
        $('#editEndDate').val('');
        $('#editIsEnabled').prop('checked', true);
        $('#modalTitle').text('新增公告');

        var myModal = new bootstrap.Modal(document.getElementById('editModal'), {
            focus: false
        });
        myModal.show();
    },

    edit: function (id) {
        const self = this;
        $.get(self.urls.getById, { id: id }, function (res) {
            if (res) {
                $('#editId').val(res.id);
                $('#editMessage').val(res.message);
                $('#editPriority').val(res.priority);
                $('#editIsEnabled').prop('checked', res.isEnabled);
                $('#editStartDate').val(res.startDate_Text);
                $('#editEndDate').val(res.endDate_Text === "長期公告" ? "" : res.endDate_Text);
                $('#modalTitle').text('編輯公告');
                $('#editModal').modal('show');
            }
        });
    },

    save: function () {
        const self = this;
        const id = parseInt($('#editId').val());
        const data = {
            id: id,
            message: $('#editMessage').val(),
            priority: parseInt($('#editPriority').val()),
            isEnabled: $('#editIsEnabled').is(':checked'),
            startDate: $('#editStartDate').val(),
            endDate: $('#editEndDate').val() === "" ? null : $('#editEndDate').val()
        };

        if (!data.message) {
            ehrisAlert.warning("請輸入公告內容");
            return;
        }

        const url = id === 0 ? self.urls.create : self.urls.update;

        Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });

        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (res) {
                Swal.close();
                ehrisAlert.handle(res).then(function () {
                    if (res.success) {
                        $('#editModal').modal('hide');
                        self.dt.ajax.reload();
                    }
                });
            },
            error: function (xhr) {
                Swal.close();
                ehrisAlert.handleError(xhr);
            }
        });
    },

    del: function (id) {
        const self = this;
        Swal.fire({
            title: '確定刪除？',
            text: '刪除後將無法復原！',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: '確定',
            cancelButtonText: '取消'
        }).then((result) => {
            if (result.isConfirmed) {
                Swal.fire({ title: '處理中...', allowOutsideClick: false, didOpen: () => { Swal.showLoading(); } });
                $.post(self.urls.delete, { id: id }, function (res) {
                    Swal.close();
                    ehrisAlert.handle(res).then(function () {
                        if (res.success) {
                            self.dt.ajax.reload();
                        }
                    });
                });
            }
        });
    }
};