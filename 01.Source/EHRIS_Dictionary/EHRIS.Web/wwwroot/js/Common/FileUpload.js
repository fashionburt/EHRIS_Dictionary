window.FileUpload = (function () {

    function init(config) {
        var cid = config.cid;
        var maxSize = config.maxSize;
        var maxSizeMB = config.maxSizeMB;
        var baseUrl = config.baseUrl;
        var uploadUrl = baseUrl + '/FileUpload/Upload';
        var removeUrl = baseUrl + '/FileUpload/RemoveFile';
        var downloadUrl = baseUrl + '/FileUpload/Download';
        var listUrl = baseUrl + '/FileUpload/GetFileList';
        var allowedExt = config.allowedExt.toLowerCase().split(',');

        var input = document.getElementById(cid + '_input');
        var uploadBtn = document.getElementById(cid + '_uploadBtn');
        var msgEl = document.getElementById(cid + '_msg');
        var tableEl = document.getElementById(cid + '_table');
        var listEl = document.getElementById(cid + '_list');
        var hiddenEl = document.getElementById(cid + '_filIds');

        // === hidden input 同步 ===
        function syncHidden() {
            var ids = [];
            listEl.querySelectorAll('tr').forEach(function (tr) {
                var id = tr.getAttribute('data-fil-id');
                if (id) ids.push(id);
            });
            hiddenEl.value = ids.join(',');
        }

        function showMsg(text, icon) {
            msgEl.textContent = text;
            if (text && typeof Swal !== 'undefined') {
                Swal.fire({ icon: icon || 'error', title: '提示', text: text });
            }
        }

        function clearMsg() { msgEl.textContent = ''; }

        function validateExtension(fileName) {
            var ext = '.' + fileName.split('.').pop().toLowerCase();
            return allowedExt.indexOf(ext) !== -1;
        }

        function formatSize(bytes) {
            if (bytes < 1024) return bytes + ' B';
            if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
            return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
        }

        function toggleTable() {
            tableEl.style.display = listEl.querySelectorAll('tr').length > 0 ? '' : 'none';
        }

        function createFileRow(filId, originalName, storedName, fileSize, uploadTime) {
            var tr = document.createElement('tr');
            tr.setAttribute('data-fil-id', filId);
            tr.setAttribute('data-stored-name', storedName);
            tr.innerHTML =
                '<td><a href="' + downloadUrl + '?fileName=' + encodeURIComponent(storedName) + '" ' +
                'class="text-primary" target="_blank">' +
                '<i class="fas fa-paperclip"></i> ' + originalName + '</a></td>' +
                '<td>' + formatSize(fileSize) + '</td>' +
                '<td>' + uploadTime + '</td>' +
                '<td><button type="button" class="btn btn-sm btn-outline-danger btn-remove-file" ' +
                'data-stored-name="' + storedName + '">' +
                '<i class="fas fa-times"></i> 刪除</button></td>';
            return tr;
        }

        // 載入既有檔案 (依 fil_id 清單)
        function loadExistingFiles() {
            var filIds = (config.filIds || '').trim();
            if (!filIds) return;

            fetch(listUrl + '?filIds=' + encodeURIComponent(filIds))
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (data && data.length > 0) {
                        data.forEach(function (f) {
                            var row = createFileRow(f.filId, f.originalName, f.storedName, f.fileSize, f.uploadTime);
                            listEl.appendChild(row);
                        });
                        toggleTable();
                        syncHidden();
                    }
                })
                .catch(function () { });
        }

        loadExistingFiles();

        // 上傳
        uploadBtn.addEventListener('click', function () {
            clearMsg();

            if (!input.files || input.files.length === 0) {
                showMsg('請先選擇要上傳的檔案。', 'warning');
                return;
            }

            var fileArray = Array.from(input.files);
            uploadBtn.disabled = true;
            var pending = 0;

            for (var i = 0; i < fileArray.length; i++) {
                var file = fileArray[i];

                if (!validateExtension(file.name)) {
                    showMsg('檔案「' + file.name + '」的類型不允許上傳。');
                    continue;
                }
                if (file.size > maxSize) {
                    showMsg('檔案「' + file.name + '」大小超過 ' + maxSizeMB + 'MB 的限制。');
                    continue;
                }

                pending++;

                (function (f) {
                    var formData = new FormData();
                    formData.append('file', f);
                    formData.append('componentId', cid);

                    var tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
                    if (tokenEl) formData.append('__RequestVerificationToken', tokenEl.value);

                    fetch(uploadUrl, { method: 'POST', body: formData })
                        .then(function (res) { return res.json(); })
                        .then(function (data) {
                            if (data.success) {
                                var row = createFileRow(
                                    data.filId,
                                    data.originalName,
                                    data.storedName,
                                    data.fileSize || f.size,
                                    data.uploadTime || new Date().toLocaleString('zh-TW', { hour12: false })
                                );
                                listEl.appendChild(row);
                                toggleTable();
                                syncHidden();
                            } else {
                                showMsg(data.message || '上傳失敗');
                            }
                        })
                        .catch(function () { showMsg('上傳過程發生錯誤'); })
                        .finally(function () {
                            pending--;
                            if (pending <= 0) {
                                input.value = '';
                                uploadBtn.disabled = false;
                            }
                        });
                })(file);
            }

            if (pending === 0) uploadBtn.disabled = false;
        });

        // 刪除
        listEl.addEventListener('click', function (e) {
            var btn = e.target.closest('.btn-remove-file');
            if (!btn) return;

            var storedName = btn.getAttribute('data-stored-name');
            var row = btn.closest('tr');

            if (!removeUrl) {
                if (row) row.remove();
                toggleTable();
                syncHidden();
                return;
            }

            var tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenEl ? tokenEl.value : '';

            fetch(removeUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': tokenValue },
                body: JSON.stringify({ storedName: storedName, componentId: cid })
            })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (data.success) {
                    if (row) row.remove();
                    toggleTable();
                    syncHidden();
                } else {
                    showMsg(data.message || '刪除失敗');
                }
            })
            .catch(function () { showMsg('刪除過程發生錯誤'); });
        });

        // 全域方法：取得已���傳檔案清單
        window[cid + '_getFiles'] = function () {
            var rows = listEl.querySelectorAll('tr');
            var files = [];
            rows.forEach(function (tr) {
                var link = tr.querySelector('a');
                files.push({
                    filId: tr.getAttribute('data-fil-id'),
                    storedName: tr.getAttribute('data-stored-name'),
                    originalName: link ? link.textContent.trim() : ''
                });
            });
            return files;
        };
    }

    return { init: init };
})();
