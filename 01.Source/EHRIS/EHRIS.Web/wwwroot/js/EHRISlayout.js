/**
 * EHRIS Layout JavaScript
 * 整合：動態載入、選單控制、系統切換、密碼管理、手機版階層網格
 */

// 全域狀態追蹤
window._currentSysNo = null;
window._isMenuLoading = false;
window._mobileMenuStack = []; // 用於紀錄手機版選單路徑
window._systemName = document.querySelector('.brand-text b')?.innerText || "功能切換";
// ==========================================
// 1. 工具函式 (Utilities)
// ==========================================

function htmlEncode(value) {
    if (!value) return '';
    return String(value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function getFinalUrl(url) {
    if (!url) return '';
    let cleanUrl = url.trim();
    if (cleanUrl.startsWith('~/')) cleanUrl = cleanUrl.substring(2);
    else if (cleanUrl.startsWith('~')) cleanUrl = cleanUrl.substring(1);

    const base = (typeof appRoot !== 'undefined' && appRoot) ? appRoot : '/';
    const baseUrl = base.endsWith('/') ? base : base + '/';
    const relativeUrl = cleanUrl.startsWith('/') ? cleanUrl.substring(1) : cleanUrl;

    return baseUrl + relativeUrl;
}

// ==========================================
// 2. 頁面載入邏輯 (Loading Logic)
// ==========================================

function loadAspx(url) {
    const mvc = document.getElementById('mvcContent');
    if (mvc) mvc.style.display = 'none';
    const iframeSection = document.getElementById('iframeContent');
    if (iframeSection) {
        iframeSection.style.display = 'block';
        document.getElementById('mainFrame').src = url;
    }
}

async function loadMvc(url, options = {}) {
    if (!url) return;
    const finalUrl = getFinalUrl(url);

    // 清除殘留的 Bootstrap Modal 遮罩（避免換頁後畫面無法操作）
    document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
    document.body.classList.remove('modal-open');
    document.body.style.overflow = '';
    document.body.style.paddingRight = '';

    Swal.fire({
        title: '資料載入中，請稍候...',
        allowOutsideClick: false,
        didOpen: () => { Swal.showLoading(); }
    });

    try {
        const method = (options.method || "GET").toUpperCase();
        let fetchOptions = {
            method,
            headers: { "X-Requested-With": "XMLHttpRequest" }
        };

        if (method === "POST") {
            fetchOptions.headers["Content-Type"] = "application/x-www-form-urlencoded; charset=UTF-8";
            fetchOptions.body = new URLSearchParams(options.data || {}).toString();
        }

        const response = await fetch(finalUrl, fetchOptions);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);

        const html = await response.text();
        const contentArea = document.querySelector("#mvcContent");
        if (contentArea) {
            const iframeSection = document.getElementById('iframeContent');
            if (iframeSection) {
                iframeSection.style.display = 'none';
            }
            contentArea.style.display = 'block';
            contentArea.innerHTML = html;

            // 依序載入 Script（外部 src 等 onload 再執行下一個，避免相依順序問題）
            const scripts = contentArea.querySelectorAll("script");
            for (let oldScript of scripts) {
                await new Promise((resolve) => {
                    const s = document.createElement("script");
                    if (oldScript.src) {
                        // 已存在相同 src 則跳過重複載入
                        if (document.querySelector(`script[src="${oldScript.src}"]`)) {
                            resolve();
                            return;
                        }
                        s.src = oldScript.src;
                        s.onload = resolve;
                        s.onerror = resolve;
                    } else {
                        s.textContent = oldScript.textContent;
                    }
                    document.body.appendChild(s);
                    if (!oldScript.src) resolve();
                });
            }
            await initPage(finalUrl);
        }
    } catch (err) {
        showError("頁面載入失敗");
    } finally {
        Swal.close();
    }
}

async function initPage(url) {
    if ($.fn.tooltip) $('[data-bs-toggle="tooltip"]').tooltip();
    if ($.fn.DataTable) {
        $('.datatable:not(.initialized)').each(function () {
            $(this).addClass('initialized').DataTable({ paging: true, searching: true });
        });
    }

    // ===== 自動載入該頁專屬 JS 並呼叫 init() =====
    const path = new URL(url, window.location.origin).pathname;
    const parts = path.split('/').filter(p => p);
    if (parts.length >= 2) {
        const controller = parts[parts.length - 2];
        const pageKey = parts[parts.length - 1];
        const folder = window.projectJsFolderMap?.[controller];
        if (!folder) return;

        const base = (typeof appRoot !== 'undefined' && appRoot) ? appRoot : '/';
        const relativePath = `/js/${folder}/${pageKey}.js`;
        // 比照二代系統 GetVersionedResource：附加檔案版本戳記，改版後自動略過瀏覽器快取
        const versionedPath = window.jsFileVersions?.[relativePath] || relativePath;
        const scriptPath = base.replace(/\/$/, '') + versionedPath;
        try {
            await loadPageScript(scriptPath, pageKey);
        } catch (e) {
            // ignore
        }
    }
}

function loadPageScript(path, pageKey) {
    return new Promise((resolve) => {
        const existing = document.querySelector(`script[src="${path}"]`);
        if (existing) {
            if (window[pageKey] && window[pageKey].init) {
                window[pageKey].init();
            }
            resolve();
            return;
        }
        const script = document.createElement('script');
        script.src = path;
        script.onload = function () {
            if (window[pageKey] && window[pageKey].init) {
                window[pageKey].init();
            }
            resolve();
        };
        script.onerror = resolve;
        document.body.appendChild(script);
    });
}

// ==========================================
// 3. 事件綁定 (Event Bindings)
// ==========================================

document.addEventListener("DOMContentLoaded", function () {
    console.log("[EHRISlayout] 初始化啟動...");

    // 主選單點擊
    $(document).on('click', '.module-link', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();

        const sysNo = $(this).data('sys-no');
        if (window._isMenuLoading) return;

        // 檢查是否真的需要重新載入
        const sidebarItemsCount = $('#sidebar-container .nav-item').length;
        if (window._currentSysNo === sysNo && sidebarItemsCount > 0) {
            console.log("[Header] 系統已啟用且側邊欄已有內容，不重複執行");
            return;
        }

        console.log("[Header] 切換系統模組:", sysNo);
        loadSystemModule(sysNo);
    });

    // 側邊欄 Treeview
    $(document).on('click', '.nav-item.has-treeview > a', function (e) {
        e.preventDefault();
        const $item = $(this).closest(".nav-item");
        const $tree = $item.find("> .nav-treeview");

        if ($tree.hasClass('d-none')) {
            $item.siblings().find('.nav-treeview').addClass('d-none');
            $item.siblings().removeClass('menu-open');
            $tree.removeClass('d-none');
            $item.addClass('menu-open');
        } else {
            $tree.addClass('d-none');
            $item.removeClass('menu-open');
        }
    });
    // 側邊欄釘選圖示狀態切換
    const sidebarIcon = document.getElementById("iconSidebar");
    const btnSidebar = document.getElementById("btnSidebar");

    if (sidebarIcon && btnSidebar) {
        const updateSidebarIcon = () => {
            // 注意：AdminLTE 切換 sidebar-collapse 類別是在 body 上
            if (document.body.classList.contains("sidebar-collapse")) {
                // Sidebar 是收起的 → 顯示未釘住 (空心圖示)
                sidebarIcon.classList.remove("bi-pin-angle-fill");
                sidebarIcon.classList.add("bi-pin-angle");
            } else {
                // Sidebar 是展開的 → 顯示已釘住 (實心圖示)
                sidebarIcon.classList.remove("bi-pin-angle");
                sidebarIcon.classList.add("bi-pin-angle-fill");
            }
        };

        // 初始化圖示狀態
        updateSidebarIcon();

        // 監聽側邊欄切換按鈕
        btnSidebar.addEventListener("click", () => {
            // 使用 setTimeout 確保在 AdminLTE 完成 Class 切換後再檢查狀態
            setTimeout(updateSidebarIcon, 150);
        });
    }
    initHeaderMenu();
    initMobileLauncher();
});

// ==========================================
// 5. 系統切換與 Header 選單
// ==========================================

function initHeaderMenu() {
    const target = document.getElementById('header-module-nav');
    if (!target) return;

    // 抓取最新的來源
    const sources = document.querySelectorAll('#sidebar-header-source');
    if (sources.length === 0) {
        // 如果抓不到，嘗試從備援重試
        if (!window._headerRetry) window._headerRetry = 0;
        if (window._headerRetry < 5) {
            window._headerRetry++;
            setTimeout(initHeaderMenu, 500);
        }
        return;
    }

    // 抓最後一個產生的 Source (通常是 AJAX 回傳後最新的那個)
    const latestSource = sources[sources.length - 1];
    const items = $(latestSource).children('li.nav-item').clone();

    if (items.length > 0) {
        items.find('.nav-treeview, .nav-arrow').remove();
        $(target).empty().append(items);

        // 同步 Active 狀態
        if (window._currentSysNo === null) {
            const $active = $(target).find('.module-link.active');
            if ($active.length > 0) window._currentSysNo = $active.data('sys-no');
        } else {
            $(target).find('.module-link').removeClass('active fw-bold text-primary border-bottom border-primary');
            $(target).find(`.module-link[data-sys-no="${window._currentSysNo}"]`).addClass('active fw-bold text-primary border-bottom border-primary');
        }

        initScrollButtons();
    }
}

async function loadSystemModule(sysNo, callback) {
    window._isMenuLoading = true;
    Swal.fire({ title: '正在切換系統...', allowOutsideClick: false, didOpen: () => Swal.showLoading() });

    try {
        const url = getFinalUrl('Home/GetMenu?sys_no=' + sysNo + '&t=' + Date.now());
        const data = await $.get(url);

        // 1. 強制清空舊有的隱藏源，防止 ID 衝突導致抓錯選單
        $('#sidebar-header-source').remove();

        // 2. 解析回傳內容
        const $newData = $('<div>').append($.parseHTML(data, document, true));
        const $newSidebar = $newData.find('#sidebar-container');
        const $newHeaderSource = $newData.find('#sidebar-header-source');

        // 3. 更新側邊欄
        const $currentSidebar = $('#sidebar-container');
        if ($newSidebar.length > 0) {
            if ($currentSidebar.length > 0) {
                $currentSidebar.replaceWith($newSidebar);
            } else {
                $('aside.app-sidebar').replaceWith($newSidebar);
            }
            // 重要：一定要在 DOM 替換完成後才標記 SysNo
            window._currentSysNo = sysNo;
            console.log("[loadSystemModule] 側邊欄替換成功, SysNo:", sysNo);
        }

        // 4. 更新隱藏源 (這會被 append 到 body，讓 initHeaderMenu 能抓到最新的)
        if ($newHeaderSource.length > 0) {
            $('body').append($newHeaderSource);
        }

        // 5. 重新渲染導覽列與側邊欄事件
        initHeaderMenu();
        reinitSidebarEvents();

        if (typeof callback === 'function') {
            setTimeout(callback, 50);
        } else {
            $('#mobile-launcher-overlay').fadeOut('fast');
        }

        Swal.close();
    } catch (xhr) {
        console.error("[loadSystemModule] 失敗:", xhr);
        Swal.close();
        showError("無法載入選單");
    } finally {
        window._isMenuLoading = false;
    }
}

// ==========================================
// 5.1 手機版方塊選單
// ==========================================

/**
 * 渲染方塊網格內容
 */
function renderMobileGrid($source, level, title) {
    const grid = document.getElementById('mobile-grid-content');
    const headerTitle = document.querySelector('#mobile-launcher-overlay h5');
    const btnBack = document.getElementById('btn-back-launcher');
    if (!grid || !headerTitle) return;

    headerTitle.innerText = title || "功能切換";

    // 修正：根據階層顯示返回鍵
    if (level === 'L1') {
        $(btnBack).addClass('d-none');
    } else {
        $(btnBack).removeClass('d-none');
    }

    let html = '';
    if (level === 'L1') {
        const $items = $($source).find('.module-link');
        $items.each(function () {
            const sysNo = $(this).data('sys-no');
            const name = $(this).find('.module-text').text().trim() || $(this).text().trim();
            const icon = $(this).find('.module-icon-wrapper').html() || $(this).find('i').prop('outerHTML') || '<i class="bi bi-grid"></i>';
            html += `<div class="launcher-item" data-level="L1" data-sys-no="${sysNo}"><div class="icon-box">${icon}</div><span>${name}</span></div>`;
        });
    } else {
        // 前兩字彩色方塊色系（與電腦版側欄 nth-child 一致）
        const charColors = [
            ['#3b82f6', '#1d4ed8'],
            ['#10b981', '#047857'],
            ['#f59e0b', '#b45309'],
            ['#8b5cf6', '#6d28d9'],
            ['#0d9488', '#0f766e'],
            ['#ec4899', '#be185d'],
        ];
        $($source).each(function (idx) {
            const $link = $(this).find('> .nav-link');
            const name = $link.find('p').text().trim() || $link.text().trim();
            const type = $(this).hasClass('has-treeview') ? 'folder' : 'file';
            // 取前兩字（與電腦版側欄邏輯相同）
            const chars = name.length >= 2 ? name.substring(0, 2) : (name.length > 0 ? name : '?');
            const [c1, c2] = charColors[idx % 6];
            const iconBox = `<div class="icon-box char-icon-box" style="background:linear-gradient(135deg,${c1},${c2});border:none;">` +
                `<span style="color:#fff;font-size:0.78rem;font-weight:900;font-family:'Microsoft JhengHei',sans-serif;letter-spacing:-0.5px;">${chars}</span>` +
                `</div>`;
            // 直接儲存 onclick / href，避免後續用文字比對失敗
            const linkOnclick = htmlEncode($link.attr('onclick') || '');
            const linkHref = htmlEncode(($link.attr('href') && $link.attr('href') !== '#') ? $link.attr('href') : '');
            html += `<div class="launcher-item" data-level="${level}" data-action="${type}" data-idx="${idx}" data-link-onclick="${linkOnclick}" data-link-href="${linkHref}">${iconBox}<span>${name}</span></div>`;
        });
    }
    grid.innerHTML = html || '<div class="text-white p-3 text-center w-100">暫無資料</div>';
}

function initMobileLauncher() {
    // 確保返回按鈕存在且初始化
    let btnBack = document.getElementById('btn-back-launcher');
    if (!btnBack) {
        const header = document.querySelector('.launcher-header');
        if (header) {
            const backHtml = '<button type="button" class="btn btn-link text-white p-0 d-none" id="btn-back-launcher"><i class="bi bi-chevron-left fs-4"></i></button>';
            $(header).prepend(backHtml);
            btnBack = document.getElementById('btn-back-launcher');
        }
    }

    $('#btn-mobile-launcher').off('click').on('click', function () {
        window._mobileMenuStack = []; // 開啟時清空堆疊
        const sources = document.querySelectorAll('#sidebar-header-source');
        const activeSource = sources.length > 0 ? sources[sources.length - 1] : document.getElementById('header-module-nav');
        renderMobileGrid(activeSource, 'L1', '功能切換');
        $('#mobile-launcher-overlay').removeClass('d-none').fadeIn('fast').css('display', 'flex');
    });

    // 返回按鈕點擊事件：使用堆疊機制回退
    $(btnBack).off('click').on('click', function () {
        if (window._mobileMenuStack.length > 0) {
            const lastState = window._mobileMenuStack.pop();
            renderMobileGrid(lastState.source, lastState.level, lastState.title);
        } else {
            // 如果堆疊沒了，回 L1
            const sources = document.querySelectorAll('#sidebar-header-source');
            const activeSource = sources.length > 0 ? sources[sources.length - 1] : document.getElementById('header-module-nav');
            renderMobileGrid(activeSource, 'L1', '功能切換');
        }
    });

    $('#mobile-grid-content').off('click', '.launcher-item').on('click', '.launcher-item', function () {
        const $item = $(this);
        const currentLevel = $item.data('level');
        const title = $item.children('span').text();
        const headerTitle = document.querySelector('#mobile-launcher-overlay h5').innerText;

        // 紀錄目前狀態到堆疊中，以便稍後返回
        const currentState = {
            level: currentLevel,
            title: headerTitle,
            source: null
        };

        if (currentLevel === 'L1') {
            const sources = document.querySelectorAll('#sidebar-header-source');
            currentState.source = sources.length > 0 ? sources[sources.length - 1] : document.getElementById('header-module-nav');
            window._mobileMenuStack.push(currentState);

            loadSystemModule($item.data('sys-no'), function () {
                const $l2 = $('#sidebar-container .nav.sidebar-menu > .nav-item');
                renderMobileGrid($l2, 'L2', title);
            });
        }
        else if (currentLevel === 'L2' && $item.data('action') === 'folder') {
            const $l2Items = $('#sidebar-container .nav.sidebar-menu > .nav-item');
            currentState.source = $l2Items;
            window._mobileMenuStack.push(currentState);

            const index = $item.data('idx');
            const $l3 = $l2Items.eq(index).find('.nav-treeview > .nav-item');
            renderMobileGrid($l3, 'L3', title);
        }
        else {
            // 執行最終功能頁面跳轉
            const $target = $('#sidebar-container').find(`.nav-link`).filter(function () {
                return $(this).text().trim() === title;
            }).first();

            const onclick = $target.attr('onclick');
            if (onclick) eval(onclick.replace('loadMvc', 'window.loadMvc'));
            $('#mobile-launcher-overlay').fadeOut('fast');
        }
    });

    $('#btn-close-launcher').on('click', () => $('#mobile-launcher-overlay').fadeOut('fast'));

    // 切換至電腦版（≥768px）時自動關閉 Launcher overlay
    const desktopQuery = window.matchMedia('(min-width: 768px)');
    const closeLauncherOnDesktop = (e) => {
        if (e.matches) {
            const $overlay = $('#mobile-launcher-overlay');
            if ($overlay.is(':visible')) {
                $overlay.fadeOut('fast');
            }
        }
    };
    desktopQuery.addEventListener('change', closeLauncherOnDesktop);
}

// ==========================================
// 6. UI 輔助功能
// ==========================================

function initScrollButtons() {
    const nav = document.getElementById('header-module-nav');
    if (!nav) return;
    $('#nav-scroll-left').off('click').on('click', () => nav.scrollBy({ left: -250, behavior: 'smooth' }));
    $('#nav-scroll-right').off('click').on('click', () => nav.scrollBy({ left: 250, behavior: 'smooth' }));
    nav.addEventListener('scroll', updateNavArrows);
    // 監聽視窗縮放，重新判斷是否需要箭頭
    window.addEventListener('resize', updateNavArrows);
    // 初始判斷（DOM 繪製後再量）
    requestAnimationFrame(updateNavArrows);
}

function updateNavArrows() {
    const nav = document.getElementById('header-module-nav');
    const l = document.getElementById('nav-scroll-left'), r = document.getElementById('nav-scroll-right');
    if (!nav || !l || !r) return;
    const hasOverflow = nav.scrollWidth > nav.clientWidth + 4;
    // 沒有溢出：兩顆箭頭全隱藏
    if (!hasOverflow) {
        l.style.display = 'none';
        r.style.display = 'none';
        return;
    }
    // 有溢出：依捲動位置決定各箭頭顯示
    l.style.display = nav.scrollLeft > 10 ? 'flex' : 'none';
    r.style.display = nav.scrollLeft + nav.clientWidth < nav.scrollWidth - 10 ? 'flex' : 'none';
}

function reinitSidebarEvents() {
    $('.nav-treeview').addClass('d-none');
    $('.nav-item').removeClass('menu-open');
}

function showError(msg) { Swal.fire({ icon: 'error', title: '錯誤', html: htmlEncode(msg) }); }