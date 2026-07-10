/**
 * EHRIS Layout JavaScript
 * 整合：動態載入、選單控制、系統切換、密碼管理、手機版方塊選單
 */

// 全域狀態追蹤
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

            const scripts = contentArea.querySelectorAll("script");
            for (let oldScript of scripts) {
                await new Promise((resolve) => {
                    const s = document.createElement("script");
                    if (oldScript.src) {
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

    const path = new URL(url, window.location.origin).pathname;
    const parts = path.split('/').filter(p => p);
    if (parts.length >= 2) {
        const controller = parts[parts.length - 2];
        const pageKey = parts[parts.length - 1];
        const folder = window.projectJsFolderMap?.[controller];
        if (!folder) return;

        const base = (typeof appRoot !== 'undefined' && appRoot) ? appRoot : '/';
        const relativePath = `/js/${folder}/${pageKey}.js`;
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

    // 頂層圖示點擊：直接依 sys_path 跳轉，不再切換系統/抓子選單
    $(document).on('click', '.module-link', function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();

        const sysPath = $(this).data('sys-path');
        if (!sysPath) return;

        loadMvc(sysPath);
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
            if (document.body.classList.contains("sidebar-collapse")) {
                sidebarIcon.classList.remove("bi-pin-angle-fill");
                sidebarIcon.classList.add("bi-pin-angle");
            } else {
                sidebarIcon.classList.remove("bi-pin-angle");
                sidebarIcon.classList.add("bi-pin-angle-fill");
            }
        };

        updateSidebarIcon();

        btnSidebar.addEventListener("click", () => {
            setTimeout(updateSidebarIcon, 150);
        });
    }
    initHeaderMenu();
    initMobileLauncher();
});

// ==========================================
// 5. Header 選單
// ==========================================

function initHeaderMenu() {
    const target = document.getElementById('header-module-nav');
    if (!target) return;

    const sources = document.querySelectorAll('#sidebar-header-source');
    if (sources.length === 0) {
        if (!window._headerRetry) window._headerRetry = 0;
        if (window._headerRetry < 5) {
            window._headerRetry++;
            setTimeout(initHeaderMenu, 500);
        }
        return;
    }

    const latestSource = sources[sources.length - 1];
    const items = $(latestSource).children('li.nav-item').clone();

    if (items.length > 0) {
        items.find('.nav-treeview, .nav-arrow').remove();
        $(target).empty().append(items);
        initScrollButtons();
    }
}

// ==========================================
// 5.1 手機版方塊選單
// ==========================================

function renderMobileGrid($source) {
    const grid = document.getElementById('mobile-grid-content');
    if (!grid) return;

    let html = '';
    const $items = $($source).find('.module-link');
    $items.each(function () {
        const sysPath = $(this).data('sys-path');
        const name = $(this).find('.module-text').text().trim() || $(this).text().trim();
        const icon = $(this).find('.module-icon-wrapper').html() || $(this).find('i').prop('outerHTML') || '<i class="bi bi-grid"></i>';
        html += `<div class="launcher-item" data-sys-path="${htmlEncode(sysPath)}"><div class="icon-box">${icon}</div><span>${name}</span></div>`;
    });

    grid.innerHTML = html || '<div class="text-white p-3 text-center w-100">暫無資料</div>';
}

function initMobileLauncher() {
    $('#btn-mobile-launcher').off('click').on('click', function () {
        const sources = document.querySelectorAll('#sidebar-header-source');
        const activeSource = sources.length > 0 ? sources[sources.length - 1] : document.getElementById('header-module-nav');
        renderMobileGrid(activeSource);
        $('#mobile-launcher-overlay').removeClass('d-none').fadeIn('fast').css('display', 'flex');
    });

    $('#mobile-grid-content').off('click', '.launcher-item').on('click', '.launcher-item', function () {
        const sysPath = $(this).data('sys-path');
        if (sysPath) {
            loadMvc(sysPath);
        }
        $('#mobile-launcher-overlay').fadeOut('fast');
    });

    $('#btn-close-launcher').on('click', () => $('#mobile-launcher-overlay').fadeOut('fast'));

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
    window.addEventListener('resize', updateNavArrows);
    requestAnimationFrame(updateNavArrows);
}

function updateNavArrows() {
    const nav = document.getElementById('header-module-nav');
    const l = document.getElementById('nav-scroll-left'), r = document.getElementById('nav-scroll-right');
    if (!nav || !l || !r) return;
    const hasOverflow = nav.scrollWidth > nav.clientWidth + 4;
    if (!hasOverflow) {
        l.style.display = 'none';
        r.style.display = 'none';
        return;
    }
    l.style.display = nav.scrollLeft > 10 ? 'flex' : 'none';
    r.style.display = nav.scrollLeft + nav.clientWidth < nav.scrollWidth - 10 ? 'flex' : 'none';
}

function showError(msg) { Swal.fire({ icon: 'error', title: '錯誤', html: htmlEncode(msg) }); }