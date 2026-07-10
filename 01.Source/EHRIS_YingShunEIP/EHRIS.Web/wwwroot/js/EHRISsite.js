/* ═══════════════════════════════════════════════════════
   共用 SweetAlert2 訊息方法
   ─────────────────────────────────────────────────────
   用法：
     ehrisAlert.error('帳號或密碼錯誤');
     ehrisAlert.success('儲存成功');
     ehrisAlert.warning('請確認資料');
     ehrisAlert.info('提示訊息');
     ehrisAlert.info('即將返回首頁', '/Home/Index');  ← 按確定後導向

   AJAX API 操作回應：
     ehrisAlert.handle(res);          ← { success, message }
     ehrisAlert.handleError(xhr);     ← jQuery xhr 物件
═══════════════════════════════════════════════════════ */
window.ehrisAlert = (function () {
    'use strict';

    var configMap = {
        error:   { icon: 'error',   title: '錯誤訊息' },
        success: { icon: 'success', title: '操作成功' },
        warning: { icon: 'warning', title: '注意' },
        info:    { icon: 'info',    title: '提示' }
    };

    /** 核心方法：彈出 SweetAlert2，按確定後可選導向 */
    function show(type, message, redirectUrl) {
        var cfg = configMap[type] || configMap.info;
        return Swal.fire({
            icon: cfg.icon,
            title: cfg.title,
            text: message,
            confirmButtonText: '確定',
            allowOutsideClick: false
        }).then(function () {
            if (redirectUrl) { window.location.href = redirectUrl; }
        });
    }

    /**
     * 處理 AJAX 成功回應 { success: bool, message: string }
     * 成功時顯示 success，失敗時顯示 error
     */
    function handle(res, redirectUrl) {
        if (res && res.success) {
            return show('success', res.message || '操作成功', redirectUrl);
        } else {
            return show('error', (res && res.message) || '操作失敗');
        }
    }

    /** 處理 AJAX error callback 的 xhr 物件 */
    function handleError(xhr) {
        var msg = (xhr.responseJSON && xhr.responseJSON.message)
            ? xhr.responseJSON.message
            : '發生未預期的錯誤，請聯絡管理員。';
        return show('error', msg);
    }

    return {
        error:       function (msg, url) { return show('error',   msg, url); },
        success:     function (msg, url) { return show('success', msg, url); },
        warning:     function (msg, url) { return show('warning', msg, url); },
        info:        function (msg, url) { return show('info',    msg, url); },
        show:        show,
        handle:      handle,
        handleError: handleError
    };
})();

window.initSelect2 = function ($scope) {
    $scope = $scope || $(document);

    $scope.find('select[data-select2="true"]:not([disabled]):not([hidden])').each(function () {
        const $select = $(this);
        
        $select.select2({
            placeholder: $select.attr('placeholder') || '請選擇',
            allowClear: true,
            closeOnSelect: true,
            dropdownPosition: 'below',
            minimumResultsForSearch: 10,
            width: 'resolve',
            theme: 'bootstrap-5'   
        });
    });
};
 
/* 匯出js 功能 */
/**
 *  
 * 
 */
const EHRIS_Export = (function () {
     
    function exportExcelPost(url, queryObj, format = 'Xlsx') {
        Swal.fire({
            title: '匯出中...',
            html: `
            <div id="progressText" style="margin-bottom: 5px;">0%</div>
            <div class="progress" style="height: 20px;">
                <div class="progress-bar" role="progressbar" style="width: 0%;" id="progressBar"></div>
            </div>
        `,
            allowOutsideClick: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
                setTimeout(() => simulateProgress(), 100);
            }
        });

        const formData = new FormData();
        for (const key in queryObj) {
            formData.append(key, queryObj[key]);
        }
        formData.append("ExportFormat", format);
 
        fetch(url, {
            method: 'POST',
            body: formData
        })
            .then(response => {
                if (!response.ok) throw new Error("匯出失敗");

                // 解析 Content-Disposition 中的檔名 
                const disposition = response.headers.get('Content-Disposition');
                let filename = '匯出結果.xlsx'; // 預設值

                if (disposition) {
                    // 優先解析 filename*=UTF-8'' 格式
                    const utf8Match = disposition.match(/filename\*=UTF-8''([^;\n]*)/);
                    if (utf8Match && utf8Match[1]) {
                        filename = decodeURIComponent(utf8Match[1]);
                    } else {
                        // 備案：解析 filename="..." 格式
                        const asciiMatch = disposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
                        if (asciiMatch && asciiMatch[1]) {
                            filename = asciiMatch[1].replace(/['"]/g, '');
                        }
                    }
                }
                 
                return response.blob().then(blob => ({ blob, filename }));
            })
            .then(({ blob, filename }) => {
                const link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = filename;
                document.body.appendChild(link);
                link.click();
                link.remove();

                //關閉原本的 loading 視窗
                Swal.close();

                //顯示匯出成功訊息
                Swal.fire({
                    icon: 'success',
                    title: '匯出完成',
                    text: '檔案已成功下載',
                    timer: 2500
                });

            })
            .catch(error => {
                Swal.fire({
                    icon: 'error',
                    title: '匯出失敗',
                    text: error.message || '請稍後再試',
                });
            });
    }

    function simulateProgress() {
        const bar = document.getElementById('progressBar');
        const text = document.getElementById('progressText');
        if (!bar || !text) return;

        let percent = 0;
        const interval = setInterval(() => {
            percent += Math.floor(Math.random() * 10) + 5;
            if (percent >= 100) percent = 100;
            bar.style.width = percent + '%';
            text.innerText = percent + '%';
            if (percent === 100) clearInterval(interval);
        }, 300);
    }


    return {
        //exportExcel,
        exportExcelPost
    };

})();


/* DataTable 功能 */

/**
 * createEhrisTable — 共用 DataTable 初始化工廠
 *
 * @param {string} tableId   — table 的 HTML id（不含 #）
 * @param {object} config    — 設定物件，欄位如下：
 *   ajaxUrl      {string}   必填，server-side 資料來源 URL
 *   columns      {Array}    必填，DataTables columns 定義
 *   searchableCols {Array}  選填，可搜尋欄位 [{index, label}]，預設 []
 *   extraData    {function} 選填，額外的查詢條件 fn(d) → object，會 merge 進 POST body
 *   pageLength   {number}   選填，每頁筆數，預設 10
 *   langUrl      {string}   選填，i18n JSON 路徑
 *   onInitComplete {function} 選填，initComplete 後的自訂邏輯 fn(api)
 *
 * @returns DataTables instance
 *
 * 使用範例：
 *   self.dt = createEhrisTable('adminTable', {
 *       ajaxUrl: urls.getAllAdminsUrl,
 *       langUrl: urls.dataTableLangUrl,
 *       searchableCols: [{ index: 0, label: '單位' }, { index: 1, label: '姓名' }],
 *       columns: [
 *           { data: 'unit',       className: 'dt-col-mid' },
 *           { data: 'editAction', className: 'dt-col-action', orderable: false }
 *       ],
 *       onInitComplete: function (api) {
 *           // 例如動態加入新增按鈕
 *       }
 *   });
 */
function createEhrisTable(tableId, config) {
    const searchableCols = config.searchableCols || [];
    const langUrl = config.langUrl || '/lib/datatables.net/plug-ins/i18n/zh-HANT.json';

    return $('#' + tableId).DataTable({
        serverSide: true,
        processing: true,
        searching: false,
        responsive: false,
        scrollX: false,
        pageLength: config.pageLength || 10,
        ajax: {
            url: config.ajaxUrl,
            type: 'POST',
            contentType: 'application/json',
            data: function (d) {
                const $wrapper = $('#' + tableId).closest('.dataTables_wrapper');
                const checkedCols = $wrapper.find('.column-checkbox:checked')
                    .map(function () { return parseInt($(this).val()); }).get();
                const searchValue = $wrapper.find('.multiColSearchInput').val() || '';

                const body = {
                    draw: d.draw,
                    start: d.start,
                    length: d.length,
                    orderby: d.order,
                    columns: d.columns,
                    extraSearch: {
                        columnIndexes: checkedCols,
                        searchValue: searchValue
                    }
                };

                if (typeof config.extraData === 'function') {
                    Object.assign(body, config.extraData(d));
                }

                return JSON.stringify(body);
            },
            error: function (xhr) {
                Swal.close();
                const msg = xhr.responseJSON?.message || '載入列表失敗，請聯絡管理員。';
                showError(msg);
            }
        },
        columns: config.columns,
        language: { url: langUrl },
        initComplete: function () {
            addDataTableGoToPageFeature(this);
            if (searchableCols.length > 0) {
                addColumnSearchFeature(this.api(), searchableCols, tableId);
            }
            if (typeof config.onInitComplete === 'function') {
                config.onInitComplete(this.api());
            }
        }
    });
}
/**
 * 1.「跳頁輸入框 + Go 按鈕」功能
 * 
 */
function addDataTableGoToPageFeature(tableInstance) {
    var api = tableInstance.api();
    var wrapper = $(api.table().container()).closest('.dataTables_wrapper');
    var info = wrapper.find('.dataTables_info');
    var paginate = wrapper.find('.dataTables_paginate');
    var length = wrapper.find('.dataTables_length'); //每頁N筆下拉
    var filter = wrapper.find('.dataTables_filter'); //搜尋
    wrapper.find('.row > .col-md-6:first').append(filter);
    // 建立 Go 輸入框 + 按鈕群組
    var goGroup = $('<div class="input-group input-group-sm ms-2" style="width:auto;"></div>');
    var goText1 = $('<span class="input-group-text">跳至</span>');
    var goInput = $('<input/>', {
        type: 'number',
        min: 1,
        value: 1,
        class: 'form-control form-control-sm',
        style: 'width:70px; height:34px; -moz-appearance:textfield;',
        inputmode: 'numeric'
    }).css({
        'appearance': 'none',
        '-webkit-appearance': 'none',
        '-moz-appearance': 'textfield'
    });

    var goText2 = $('<span class="input-group-text">頁</span>');
   

    // Enter 事件
    goInput.on('keypress', function (e) {
        if (e.which === 13) {
            goToPage();
        }
    });

    // Go 按鈕
    var goButton = $('<button/>', {
        class: 'btn btn-primary input-group-text btn-sm me-2',
        text: 'Go'
    });
   
    goButton.on('click', function () {
        goToPage();
    });
    goGroup.append(goText1, goInput, goText2, goButton);
    // 驗證與跳頁
    function goToPage() {
        var val = goInput.val().trim();
        if (val === '') {
            Swal.fire({ icon: 'warning', text: '請輸入頁碼' });
            return;
        }

        var page = parseInt(val, 10);
        var pageInfo = api.page.info();

        if (!isNaN(page) && page > 0 && page <= pageInfo.pages) {
            api.page(page - 1).draw('page');
        } else {
            Swal.fire({ icon: 'warning', text: '頁碼超出範圍' });
        }
    }

    var row = $('<div class="row mt-2"></div>');
    //var colLeft = $('<div class="col-md-5 d-flex align-items-center"></div>').append(length).append(info);
    var colLeft = $('<div class="col-md-5"></div>')
        .append(
            $('<div class="d-flex align-items-center gap-2"></div>')
                .append(length)
                .append(info)
        );
    var colRight = $('<div class="col-md-7 d-flex justify-content-end align-items-center"></div>')
        .append(goGroup)
        .append(paginate);
    row.append(colLeft, colRight);

    // 取代原有 info & paginate
    wrapper.find('.dataTables_info, .dataTables_paginate').remove();
    wrapper.append(row);

    // 修正 paginate 按鈕排版
    paginate.css({ display: 'flex', gap: '4px' });
}

// -------------------------
// 欄位搜尋功能 (下拉 + input)
// -------------------------
function addColumnSearchFeature(tableApi, searchableCols, tableId) {
    const uniqueId = `${tableId}_searchInput`;
    const uniqueDropdownId = `${tableId}_dropdown`;

    // 移除舊的搜尋 group（只針對當前 table）
    $(tableApi.table().container()).find('.multiColSearchWrapper').remove();

    // 建立 Dropdown
    const $dropdown = $(`
        <div class="dropdown me-2" id="${uniqueDropdownId}">
            <button class="btn btn-outline-secondary btn-sm dropdown-toggle" type="button" data-bs-toggle="dropdown">
                <i class="fas fa-bars me-1"></i>搜尋欄位(<span class="col-count">0</span>)
            </button>
            <ul class="dropdown-menu p-2" style="max-height:300px; overflow:auto; min-width:180px;"></ul>
        </div>
    `);

    const $menu = $dropdown.find(".dropdown-menu");
    const $count = $dropdown.find(".col-count");

    searchableCols.forEach(c => {
        $menu.append(`
            <li>
                <label class="dropdown-item d-flex align-items-center">
                    <input type="checkbox" class="form-check-input me-2 column-checkbox column-toggle" value="${c.index}" checked>
                    ${c.label}
                </label>
            </li>
        `);
    });

    const $input = $(`<input type="text" id="${uniqueId}" class="form-control form-control-sm multiColSearchInput" placeholder="請輸入關鍵字" style="width:200px;">`);

    const $group = $('<div class="d-flex align-items-center mb-2 multiColSearchWrapper"></div>')
        .append($dropdown)
        .append($input);

    $(tableApi.table().container()).find('.row:first .col-md-6:first').append($group);

    //只綁定當前 input
    $input.on('keyup change', function () {
        tableApi.draw();
    });

    //只綁定當前 dropdown 的 checkbox
    $dropdown.on('change', '.column-checkbox', function () {
        tableApi.draw();
        updateCount();
    });

    function updateCount() {
        const checkedCount = $menu.find(".column-toggle:checked").length;
        $count.text(checkedCount);
    }

    updateCount();

    // 暴露屬性（可供其他地方操作）
    tableApi.columnSearchDropdown = $dropdown;
    tableApi.columnSearchInput = $input;
}

// ==========================================
// 截圖功能
// ==========================================
function captureScreenshot() {
    // 關閉所有開啟的 dropdown，避免截圖包含下拉選單
    document.querySelectorAll('.dropdown-menu.show').forEach(function (el) {
        el.classList.remove('show');
    });
    document.querySelectorAll('[data-bs-toggle="dropdown"][aria-expanded="true"]').forEach(function (el) {
        el.setAttribute('aria-expanded', 'false');
    });

    // 等 dropdown 動畫收完再截圖
    setTimeout(function () {
        Swal.fire({
            title: '截圖中，請稍候...',
            allowOutsideClick: false,
            didOpen: function () { Swal.showLoading(); }
        });

        const target = document.querySelector('.app-wrapper') || document.body;

        htmlToImage.toPng(target, {
            pixelRatio: window.devicePixelRatio || 1,
            filter: function (node) {
                // 排除 SweetAlert2 overlay 與截圖按鈕，避免出現在截圖中
                if (node.classList) {
                    if (node.classList.contains('swal2-container')) return false;
                }
                return node.id !== 'btnScreenshot';
            }
        }).then(function (dataUrl) {
            Swal.close();
            const now = new Date();
            const pad = n => String(n).padStart(2, '0');
            const ts = `${now.getFullYear()}${pad(now.getMonth()+1)}${pad(now.getDate())}_${pad(now.getHours())}${pad(now.getMinutes())}${pad(now.getSeconds())}`;
            const link = document.createElement('a');
            link.download = `EHRIS_${ts}.png`;
            link.href = dataUrl;
            link.click();
        }).catch(function (err) {
            console.error('截圖失敗:', err);
            Swal.fire({ icon: 'error', title: '截圖失敗', text: '無法擷取目前畫面，請稍後再試。' });
        });
    }, 350);
}



