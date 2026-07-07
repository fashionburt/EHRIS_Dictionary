window.PeoplePhoto = (function () {

    function init(config) {
        var cid = config.cid;
        var baseUrl = config.baseUrl;
        var uploadUrl = baseUrl + '/PeoplePhoto/Upload';
        var cropUrl = baseUrl + '/PeoplePhoto/Crop';
        var previewBaseUrl = baseUrl + '/PeoplePhoto/Preview?fileName=';
        var watermarkText = config.watermarkText || '';

        var input = document.getElementById(cid + '_input');
        var uploadBtn = document.getElementById(cid + '_uploadBtn');
        var msgEl = document.getElementById(cid + '_msg');
        var previewImg = document.getElementById(cid + '_preview');
        var placeholder = document.getElementById(cid + '_placeholder');
        var cropImg = document.getElementById(cid + '_cropImg');
        var confirmBtn = document.getElementById(cid + '_confirmBtn');
        var cancelBtn = document.getElementById(cid + '_cancelBtn');
        var hiddenFileName = document.getElementById(cid + '_fileName');
        var hiddenWmFileName = document.getElementById(cid + '_wmFileName');

        var cropModalEl = document.getElementById(cid + '_cropModal');
        var cropModal = new bootstrap.Modal(cropModalEl);
        var zoomModalEl = document.getElementById(cid + '_zoomModal');
        var zoomModal = new bootstrap.Modal(zoomModalEl);
        var zoomImg = document.getElementById(cid + '_zoomImg');

        var cropper = null;
        var tempFileName = '';

        function showMsg(text) {
            msgEl.textContent = text;
            if (text && typeof Swal !== 'undefined') {
                Swal.fire({ icon: 'error', title: '提示', text: text });
            }
        }

        function clearMsg() { msgEl.textContent = ''; }

        function showPreview(url) {
            previewImg.src = url;
            previewImg.style.display = '';
            placeholder.style.display = 'none';
        }

        function hidePreview() {
            previewImg.src = '';
            previewImg.style.display = 'none';
            placeholder.style.display = '';
        }

        function destroyCropper() {
            if (cropper) {
                cropper.destroy();
                cropper = null;
            }
        }

        // 載入既有照片
        if (config.existingWmFile) {
            showPreview(previewBaseUrl + encodeURIComponent(config.existingWmFile));
        } else if (config.existingFile) {
            showPreview(previewBaseUrl + encodeURIComponent(config.existingFile));
        }

        // 上傳
        uploadBtn.addEventListener('click', function () {
            clearMsg();

            if (!input.files || input.files.length === 0) {
                showMsg('請先選擇照片');
                return;
            }

            var file = input.files[0];
            var ext = '.' + file.name.split('.').pop().toLowerCase();
            var allowed = config.allowedExtensions || ['.gif', '.png', '.bmp', '.jpg', '.jpeg'];
            if (allowed.indexOf(ext) === -1) {
                showMsg('只允許 ' + allowed.map(function (e) { return e.replace('.', '').toUpperCase(); }).join(', ') + ' 格式');
                return;
            }

            uploadBtn.disabled = true;
            var formData = new FormData();
            formData.append('file', file);

            var tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
            if (tokenEl) formData.append('__RequestVerificationToken', tokenEl.value);

            fetch(uploadUrl, { method: 'POST', body: formData })
                .then(function (res) {
                    if (!res.ok) {
                        return res.text().then(function (txt) {
                            console.error('PeoplePhoto upload response:', res.status, txt);
                            throw new Error('HTTP ' + res.status);
                        });
                    }
                    return res.json();
                })
                .then(function (data) {
                    if (data.success) {
                        tempFileName = data.tempFileName;

                        // 設定裁切圖片並開啟 Modal
                        destroyCropper();
                        cropImg.src = data.previewUrl;
                        cropModal.show();
                    } else {
                        showMsg(data.message || '上傳失敗');
                    }
                })
                .catch(function (err) { console.error('PeoplePhoto upload error:', err); showMsg('上傳過程發生錯誤：' + err.message); })
                .finally(function () { uploadBtn.disabled = false; });
        });

        // 確認裁切
        confirmBtn.addEventListener('click', function () {
            if (!cropper || !tempFileName) return;

            var cropData = cropper.getData(true); // 取得整數像素值

            confirmBtn.disabled = true;

            var tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
            var tokenValue = tokenEl ? tokenEl.value : '';

            fetch(cropUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': tokenValue
                },
                body: JSON.stringify({
                    tempFileName: tempFileName,
                    x: cropData.x,
                    y: cropData.y,
                    width: cropData.width,
                    height: cropData.height,
                    watermarkText: watermarkText
                })
            })
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (data.success) {
                    // 更新 hidden inputs
                    hiddenFileName.value = data.thumbnailFileName;
                    hiddenWmFileName.value = data.watermarkFileName || '';

                    // 顯示結果預覽（優先浮水印版）
                    var previewUrl = data.watermarkUrl || data.thumbnailUrl;
                    showPreview(previewUrl);

                    // 關閉 Modal
                    cropModal.hide();
                    destroyCropper();
                    input.value = '';
                    tempFileName = '';

                    if (typeof Swal !== 'undefined') {
                        Swal.fire({ icon: 'success', title: '完成', text: '照片裁切完成', timer: 1500, showConfirmButton: false });
                    }
                } else {
                    showMsg(data.message || '裁切失敗');
                }
            })
            .catch(function (err) { console.error('PeoplePhoto crop error:', err); showMsg('裁切過程發生錯誤：' + err.message); })
            .finally(function () { confirmBtn.disabled = false; });
        });

        // 取消裁切
        cancelBtn.addEventListener('click', function () {
            cropModal.hide();
            destroyCropper();
            input.value = '';
            tempFileName = '';
        });

        // 點擊大頭照放大預覽
        previewImg.addEventListener('click', function () {
            if (!previewImg.src || previewImg.style.display === 'none') return;
            zoomImg.src = previewImg.src;
            zoomModal.show();
        });

        // 點擊放大圖關閉
        zoomImg.addEventListener('click', function () {
            zoomModal.hide();
        });

        // Modal 完全展開後初始化 Cropper
        cropModalEl.addEventListener('shown.bs.modal', function () {
            if (cropper) return;
            cropper = new Cropper(cropImg, {
                aspectRatio: 3 / 4,
                viewMode: 1,
                dragMode: 'move',
                autoCropArea: 0.8,
                responsive: true,
                restore: false,
                guides: true,
                center: true,
                highlight: false,
                cropBoxMovable: true,
                cropBoxResizable: true,
                toggleDragModeOnDblclick: false
            });
        });

        // 全域方法
        window[cid + '_getPhoto'] = function () {
            return {
                fileName: hiddenFileName.value,
                watermarkFileName: hiddenWmFileName.value
            };
        };
    }

    return { init: init };
})();
