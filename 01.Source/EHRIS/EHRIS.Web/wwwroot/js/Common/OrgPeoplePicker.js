// OrgPeoplePicker — 上區部門樹 + 下區 DataTables(server-side) 人員選擇器
// token 模型：個別 peo_uid / d:depNo(整部門全選) / x:peo_uid(部門全選下的排除)
// 對外 API：window.{cid}_getTokens() / window.{cid}_open() / window.{cid}_clear()
if (!window.OrgPeoplePicker) window.OrgPeoplePicker = (function () {
    'use strict';
    const instances = {};

    function init(cfg) {
        if (instances[cfg.cid]) return;
        const inst = {
            cid: cfg.cid,
            fieldName: cfg.fieldName,
            ptyNo: (cfg.ptyNo === null || cfg.ptyNo === undefined) ? null : cfg.ptyNo,
            showTitle: cfg.showTitle !== false,
            multiple: cfg.multiple !== false,
            peopleStatus: (cfg.peopleStatus == null ? 1 : cfg.peopleStatus),  // 1在職 2離職 0全部
            showSelf: cfg.showSelf !== false,
            nodeType: (cfg.nodeType == null ? 4 : cfg.nodeType),   // 4=Automa(不卡權限)；權責邏輯待接
            authType: (cfg.authType == null ? 0 : cfg.authType),
            baseUrl: cfg.baseUrl || '',

            tree: null,                    // {nodes, nodeMap, childrenMap, countMap}
            treeLoaded: false,
            currentDept: 0,
            global: false,
            q: '',
            dt: null,                      // DataTable instance

            selectedDepts: new Set(),      // dep_no 全選
            excluded: new Map(),           // uid -> depNo（部門全選下排除）
            selectedPeople: new Map(),     // uid -> {name, proName, depNo, depName}（個別選）
            preCount: {},                  // dep_no -> 人數（回顯時樹未載入的 fallback）
            convertingDeps: new Set()      // 正在做「正面表列」轉換的部門（避免重複觸發）
        };

        // 回顯：把 SelectedTokens 解析出的名稱資料種回狀態
        const pre = cfg.preloaded || {};
        (pre.depts || []).forEach(d => { inst.selectedDepts.add(Number(d.id)); inst.preCount[Number(d.id)] = d.count || 0; });
        (pre.people || []).forEach(p => inst.selectedPeople.set(Number(p.id), { name: p.name, proName: p.proName, depNo: Number(p.depNo), depName: p.depName }));
        (pre.excluded || []).forEach(x => inst.excluded.set(Number(x.id), { depNo: Number(x.depNo), name: x.name || '', depName: x.depName || '' }));

        instances[cfg.cid] = inst;
        bindEvents(inst);

        // 單選模式：隱藏「全選」（單選無整批選取概念）
        if (!inst.multiple) {
            const body = $id(inst.cid, 'body'); if (body) body.classList.add('op-single');
            const da = $id(inst.cid, 'deptAll');
            if (da) { const w = da.closest('.op-dept-all'); if (w) w.style.display = 'none'; }
        }

        registerApi(inst);

        // 回顯：主頁已選區先畫出來
        if (inst.selectedDepts.size || inst.selectedPeople.size) renderMainChips(inst);
    }

    function $id(cid, s) { return document.getElementById(cid + '_' + s); }
    function debounce(fn, ms) { let t; return function () { clearTimeout(t); t = setTimeout(fn, ms); }; }

    function bindEvents(inst) {
        const cid = inst.cid;
        $id(cid, 'trigger').addEventListener('click', () => openModal(inst));
        $id(cid, 'confirm').addEventListener('click', () => confirmSel(inst));
        $id(cid, 'clear').addEventListener('click', () => clearAll(inst));

        const ts = $id(cid, 'treeSearch');
        if (ts) ts.addEventListener('input', e => filterTree(inst, e.target.value.trim().toLowerCase()));

        const ps = $id(cid, 'peopleSearch');
        if (ps) ps.addEventListener('input', debounce(function () {
            inst.q = ps.value.trim();
            reloadGrid(inst);
        }, 300));

        const g = $id(cid, 'global');
        if (g) g.addEventListener('change', e => {
            inst.global = e.target.checked;
            const label = $id(cid, 'curDept');
            if (label) label.textContent = inst.global ? '全機關搜尋' :
                (inst.tree && inst.tree.nodeMap[inst.currentDept] ? inst.tree.nodeMap[inst.currentDept].name : '請先點選左方部門');
            const hint = $id(cid, 'gridHint'); if (hint) hint.style.display = 'none';
            showPanePeople(inst);   // 手機版切到人員視圖
            ensureGrid(inst);
            reloadGrid(inst);
            updateDeptAll(inst);    // 搜全機關時停用「全選」
        });

        // 「全選」= 目前部門整批全選 / 取消（取代原本樹上的勾選框）
        const deptAll = $id(cid, 'deptAll');
        if (deptAll) deptAll.addEventListener('change', e => toggleDept(inst, inst.currentDept, e.target.checked));

        // 手機版：返回部門（切回單頁導覽的部門視圖）
        const back = $id(cid, 'back');
        if (back) back.addEventListener('click', () => showPaneTree(inst));

        // 取消（X / 取消鈕 / 點背景）關閉時，若未按「確定」則還原到進場狀態
        const modalEl = $id(cid, 'modal');
        if (modalEl) modalEl.addEventListener('hidden.bs.modal', function () {
            if (!inst._committed && inst._snap) {
                restore(inst, inst._snap);
                afterChange(inst);
                if (inst.dt) syncPageChecks(inst);
            }
        });

        // 樹事件委派：點部門列 = 載入該部門人員（無勾選框）
        const root = $id(cid, 'treeRoot');
        root.addEventListener('click', e => {
            const tg = e.target.closest('.op-node-toggle');
            if (tg) { e.stopPropagation(); tg.closest('li').classList.toggle('collapsed'); return; }
            const row = e.target.closest('.op-node-row');
            if (!row) return;
            selectDept(inst, parseInt(row.closest('li').dataset.dep, 10), row);
        });
    }

    function snapshot(inst) {
        return {
            depts: new Set(inst.selectedDepts),
            excluded: new Map(inst.excluded),
            people: new Map(inst.selectedPeople)
        };
    }
    function restore(inst, snap) {
        inst.selectedDepts = new Set(snap.depts);
        inst.excluded = new Map(snap.excluded);
        inst.selectedPeople = new Map(snap.people);
    }

    function openModal(inst) {
        if (!inst.treeLoaded) loadTree(inst);
        inst._snap = snapshot(inst);   // 進場快照：取消時還原到此
        inst._committed = false;
        showPaneTree(inst);            // 手機版每次開啟先回到部門視圖
        afterChange(inst);             // Modal 內已選/樹勾選 反映目前(已提交)狀態
        bootstrap.Modal.getOrCreateInstance($id(inst.cid, 'modal')).show();
    }

    // 手機版單頁導覽：op-show-people class 控制顯示部門 or 人員（桌面版兩區都在，class 無作用）
    function showPanePeople(inst) {
        const body = $id(inst.cid, 'body');
        if (body) body.classList.add('op-show-people');
    }
    function showPaneTree(inst) {
        const body = $id(inst.cid, 'body');
        if (body) body.classList.remove('op-show-people');
    }

    async function loadTree(inst) {
        const loading = $id(inst.cid, 'treeLoading');
        try {
            const qs = [];
            if (inst.ptyNo != null) qs.push('ptyNo=' + encodeURIComponent(inst.ptyNo));
            qs.push('peopleStatus=' + encodeURIComponent(inst.peopleStatus));
            qs.push('showSelf=' + (inst.showSelf ? 'true' : 'false'));
            qs.push('nodeType=' + encodeURIComponent(inst.nodeType));
            const url = inst.baseUrl + '/OrgPeople/Tree?' + qs.join('&');
            const res = await fetch(url, { credentials: 'same-origin', headers: { 'Accept': 'application/json' } });
            if (!res.ok) throw new Error('HTTP ' + res.status);
            const nodes = await res.json();
            inst.tree = buildIndex(nodes);
            inst.treeLoaded = true;
            renderTree(inst);
            if (loading) loading.style.display = 'none';
        } catch (err) {
            console.error('[OrgPeoplePicker] load tree failed', err);
            if (loading) loading.textContent = '載入失敗';
        }
    }

    function buildIndex(nodes) {
        const nodeMap = {}, childrenMap = {}, countMap = {};
        nodes.forEach(n => {
            n.depNo = Number(n.depNo);
            n.parentId = Number(n.parentId) || 0;
            nodeMap[n.depNo] = n;
            countMap[n.depNo] = n.peopleCount || 0;
            (childrenMap[n.parentId] = childrenMap[n.parentId] || []).push(n);
        });
        return { nodes, nodeMap, childrenMap, countMap };
    }

    function renderTree(inst) {
        const root = $id(inst.cid, 'treeRoot');
        root.innerHTML = '';
        // 頂層＝parentId 為 0 或「父層不在回傳集合內」的節點（Self/Parallel 只回子集，父層不存在）
        const tops = inst.tree.nodes.filter(n => !n.parentId || !inst.tree.nodeMap[n.parentId]);
        tops.forEach(n => root.appendChild(renderNode(inst, n)));
        refreshTreeChecks(inst);
    }

    function renderNode(inst, node) {
        const li = document.createElement('li');
        li.dataset.dep = node.depNo;
        const kids = inst.tree.childrenMap[node.depNo] || [];
        if (kids.length) li.classList.add('has-children');

        const row = document.createElement('div');
        row.className = 'op-node-row';
        row.innerHTML =
            '<i class="op-node-toggle fas fa-chevron-down"></i>' +
            '<i class="op-node-icon fas fa-folder"></i>' +
            '<span class="op-node-name"></span>' +
            '<span class="op-node-count"></span>';
        row.querySelector('.op-node-name').textContent = node.name;
        const cnt = inst.tree.countMap[node.depNo] || 0;
        row.querySelector('.op-node-count').textContent = cnt ? cnt : '';
        li.appendChild(row);

        if (kids.length) {
            const ul = document.createElement('ul');
            kids.forEach(c => ul.appendChild(renderNode(inst, c)));
            li.appendChild(ul);
        }
        return li;
    }

    function filterTree(inst, q) {
        const root = $id(inst.cid, 'treeRoot');
        if (!inst.tree) return;
        if (!q) { root.querySelectorAll('li').forEach(li => li.style.display = ''); return; }
        const match = new Set();
        inst.tree.nodes.forEach(n => {
            if ((n.name || '').toLowerCase().indexOf(q) !== -1) {
                match.add(n.depNo);
                let p = n.parentId;
                while (p) { match.add(p); p = inst.tree.nodeMap[p] ? inst.tree.nodeMap[p].parentId : 0; }
            }
        });
        root.querySelectorAll('li').forEach(li => {
            const dep = parseInt(li.dataset.dep, 10);
            li.style.display = match.has(dep) ? '' : 'none';
            if (match.has(dep)) li.classList.remove('collapsed');
        });
    }

    // ── 部門瀏覽 / 全選 ──────────────────────────────
    function selectDept(inst, depNo, rowEl) {
        inst.currentDept = depNo;
        inst.global = false;
        const g = $id(inst.cid, 'global'); if (g) g.checked = false;
        inst.q = ''; const ps = $id(inst.cid, 'peopleSearch'); if (ps) ps.value = '';

        $id(inst.cid, 'treeRoot').querySelectorAll('.op-node-row.active').forEach(r => r.classList.remove('active'));
        if (rowEl) rowEl.classList.add('active');

        const node = inst.tree.nodeMap[depNo];
        $id(inst.cid, 'curDept').textContent = node ? node.name : '';
        const hint = $id(inst.cid, 'gridHint'); if (hint) hint.style.display = 'none';

        showPanePeople(inst);   // 手機版切到人員視圖
        ensureGrid(inst);
        reloadGrid(inst);
        updateDeptAll(inst);    // 更新「全選」勾選狀態
    }

    function toggleDept(inst, depNo, checked) {
        if (!inst.multiple || !depNo || depNo <= 0) return;   // 單選 / 未選部門時無「全選」
        if (checked) {
            inst.selectedDepts.add(depNo);
            for (const [uid, info] of Array.from(inst.excluded)) if (info.depNo === depNo) inst.excluded.delete(uid);
            for (const [uid, info] of Array.from(inst.selectedPeople)) if (info.depNo === depNo) inst.selectedPeople.delete(uid);
        } else {
            inst.selectedDepts.delete(depNo);
            for (const [uid, info] of Array.from(inst.excluded)) if (info.depNo === depNo) inst.excluded.delete(uid);
        }
        afterChange(inst);
        if (inst.currentDept === depNo) syncPageChecks(inst);
    }

    // ── DataTable ────────────────────────────────────
    function gridMsg(inst, text) {
        const hint = $id(inst.cid, 'gridHint');
        if (hint) { hint.style.display = ''; hint.textContent = text; }
    }

    function ensureGrid(inst) {
        if (inst.dt) return;
        const cid = inst.cid;

        // 人員 grid 依賴 jQuery + DataTables；此頁若未載入會在這裡擋下並提示
        if (typeof window.jQuery === 'undefined' || !window.jQuery.fn || !window.jQuery.fn.DataTable) {
            gridMsg(inst, '⚠ 此頁未載入 jQuery / DataTables，人員清單無法顯示。請在有載入 DataTables 的頁面（主框架）使用。');
            console.error('[OrgPeoplePicker] jQuery / DataTables not available on this page');
            return;
        }

        const cols = [
            { data: null, orderable: false, className: 'op-check-col', render: (d) => rowCheckHtml(inst, d) },
            { data: 'depName', className: 'op-col-dep' }
        ];
        if (inst.showTitle) cols.push({ data: 'proName', className: 'op-col-pro' });
        cols.push({ data: 'name', className: 'op-col-name' });
        // 手機版合併欄（桌面隱藏）：部門 / 職稱 / 姓名 三行堆疊
        cols.push({ data: null, orderable: false, className: 'op-col-mobile', render: (d) => mobileCell(inst, d) });

        // 統一版面：改用房規 createEhrisTable（底部「每頁筆數 / 資訊 / 跳頁 / 分頁」與 SYS201001 一致）
        if (typeof window.createEhrisTable !== 'function') {
            gridMsg(inst, '⚠ 此頁未載入 createEhrisTable（EHRISsite.js），人員清單無法顯示。');
            console.error('[OrgPeoplePicker] createEhrisTable not available on this page');
            return;
        }

        inst.dt = createEhrisTable(cid + '_grid', {
            ajaxUrl: inst.baseUrl + '/OrgPeople/People',
            columns: cols,
            pageLength: 10,
            extraData: function () {
                return {
                    depNo: inst.currentDept || 0,
                    ptyNo: inst.ptyNo,
                    q: inst.q || '',
                    global: inst.global,
                    peopleStatus: inst.peopleStatus,
                    showSelf: inst.showSelf,
                    nodeType: inst.nodeType
                };
            },
            onInitComplete: function () { compactInfoMobile(inst); }   // 跳頁框「/N頁」初次即顯示
        });

        // 每次重畫（含翻頁 / reload）後重套勾選——不改共用 createEhrisTable，改掛 DataTables draw 事件
        inst.dt.on('draw', function () {
            const hint = $id(inst.cid, 'gridHint');
            if (hint) {
                if (inst.global && !inst.q) { hint.style.display = ''; hint.textContent = '請輸入搜尋關鍵字（全機關搜尋）'; }
                else if (!inst.global && (!inst.currentDept || inst.currentDept <= 0)) { hint.style.display = ''; hint.textContent = '請先點選左方部門'; }
                else hint.style.display = 'none';
            }
            syncPageChecks(inst);
            compactInfoMobile(inst);
        });

        // 點整列即切換勾選（checkbox 純顯示，CSS pointer-events:none 讓點擊落到列）
        $('#' + cid + '_grid tbody').on('click', 'tr', function () {
            const data = inst.dt.row(this).data();
            if (!data) return;
            toggleRow(inst, data, !isRowSelected(inst, data.peoUid, data.depNo));
        });
    }

    function reloadGrid(inst) {
        if (!inst.dt) ensureGrid(inst);
        if (inst.dt) inst.dt.ajax.reload(null, false);
    }

    // 手機版：資訊縮短為「1-10筆/共350筆」；跳頁框顯示總頁數，讓使用者知道可跳到第幾頁
    function compactInfoMobile(inst) {
        if (window.innerWidth > 768 || !inst.dt) return;
        const wrap = document.getElementById(inst.cid + '_grid_wrapper');
        if (!wrap) return;
        const p = inst.dt.page.info();
        const info = wrap.querySelector('.dataTables_info');
        if (info) info.textContent = p.recordsDisplay
            ? ((p.start + 1) + '-' + p.end + '筆/共' + p.recordsDisplay + '筆')
            : '共 0 筆';
        // 跳頁輸入框的「頁」字改成「/ 總頁數 頁」
        const pages = p.pages || 1;
        wrap.querySelectorAll('.input-group .input-group-text').forEach(t => {
            if (t.textContent.indexOf('頁') !== -1) t.textContent = '/' + pages + '頁';
        });
        // 上一頁/下一頁換成箭頭，避免文字太寬
        const items = wrap.querySelectorAll('.pagination .page-item');
        if (items.length) {
            const first = items[0].querySelector('.page-link');
            const last = items[items.length - 1].querySelector('.page-link');
            if (first) first.textContent = '‹';
            if (last) last.textContent = '›';
        }
    }

    function rowCheckHtml(inst, d) {
        return '<input type="checkbox" class="op-row-check" ' +
            (isRowSelected(inst, d.peoUid, d.depNo) ? 'checked' : '') + ' />';
    }

    function escapeHtml(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
    }

    // 手機版合併欄：第一行「部門 職稱」（靠左縮小），第二行姓名（靠右、大小不變）
    function mobileCell(inst, d) {
        var line1 = '<span class="op-m-dep">' + escapeHtml(d.depName) + '</span>';
        if (inst.showTitle && d.proName) line1 += '<span class="op-m-pro">' + escapeHtml(d.proName) + '</span>';
        return '<div class="op-m-line1">' + line1 + '</div>' +
               '<div class="op-m-name">' + escapeHtml(d.name) + '</div>';
    }

    function isRowSelected(inst, uid, depNo) {
        if (inst.selectedDepts.has(depNo) && !inst.excluded.has(uid)) return true;
        if (inst.selectedPeople.has(uid)) return true;
        return false;
    }

    function toggleRow(inst, data, checked) {
        applyRow(inst, data, checked);
        const removed = normalizeDept(inst, data.depNo);   // 整部門都取消 → 移除
        afterChange(inst);
        syncPageChecks(inst);   // 反映權威狀態到表格（單選時取消其他列勾選）
        if (removed) return;
        // 讓表示法永遠存「較小的一側」：
        if (inst.selectedDepts.has(data.depNo)) maybeConvertPositive(inst, data.depNo);  // d:+x：排除過半 → 轉個別
        else maybeConvertToDept(inst, data.depNo);                                       // 個別：選取過半 → 轉部門+排除
    }

    // 全選部門的人被排除到「一個不剩」時，整個部門從已選區移除
    function normalizeDept(inst, dep) {
        if (!inst.selectedDepts.has(dep)) return false;
        const total = deptTotal(inst, dep);
        let excl = 0; for (const [, info] of inst.excluded) if (info.depNo === dep) excl++;
        if (total > 0 && excl >= total) {
            inst.selectedDepts.delete(dep);
            for (const [uid, info] of Array.from(inst.excluded)) if (info.depNo === dep) inst.excluded.delete(uid);
            return true;
        }
        return false;
    }

    // 取消(排除)數 > 保留數時，改「正面表列」：抓保留者 → 轉個別選取，丟掉 d: 與該部門 x:
    var CONVERT_CAP = 500;   // 保留數超過此值不轉（避免大量 chip / SQL 參數）；此情境需手動取消上千人，不切實際
    async function maybeConvertPositive(inst, dep) {
        if (!inst.selectedDepts.has(dep) || inst.convertingDeps.has(dep)) return;
        const total = deptTotal(inst, dep);
        let excl = 0; for (const [, info] of inst.excluded) if (info.depNo === dep) excl++;
        const net = total - excl;
        if (net <= 0 || excl <= net || net > CONVERT_CAP) return;

        inst.convertingDeps.add(dep);
        try {
            const excludeUids = [];
            inst.excluded.forEach((info, uid) => { if (info.depNo === dep) excludeUids.push(uid); });
            const kept = await fetchDeptKept(inst, dep, excludeUids);
            inst.selectedDepts.delete(dep);
            for (const [uid, info] of Array.from(inst.excluded)) if (info.depNo === dep) inst.excluded.delete(uid);
            kept.forEach(p => inst.selectedPeople.set(Number(p.peoUid), { name: p.name, proName: p.proName, depNo: Number(p.depNo), depName: p.depName }));
            afterChange(inst);
            if (inst.dt) syncPageChecks(inst);
        } catch (e) {
            console.error('[OrgPeoplePicker] convert to positive failed', e);
        } finally {
            inst.convertingDeps.delete(dep);
        }
    }

    // 個別勾選數 > 該部門半數時，改「以部門為主 + 排除」：d: 整部門，未選者變 x:
    async function maybeConvertToDept(inst, dep) {
        if (!inst.multiple || !dep || dep <= 0) return;
        if (inst.selectedDepts.has(dep) || inst.convertingDeps.has(dep)) return;
        const total = deptTotal(inst, dep);
        if (!total) return;
        const selUids = [];
        inst.selectedPeople.forEach((info, uid) => { if (info.depNo === dep) selUids.push(uid); });
        if (selUids.length * 2 <= total) return;            // 未過半
        if (total - selUids.length > CONVERT_CAP) return;   // 排除側太多才不轉（極端、不切實際）

        inst.convertingDeps.add(dep);
        try {
            // 排除 = 該部門在職者 − 已選（DeptKept 回傳「未被選的人」，正好當 x:）
            const toExclude = await fetchDeptKept(inst, dep, selUids);
            inst.selectedDepts.add(dep);
            for (const [uid, info] of Array.from(inst.selectedPeople)) if (info.depNo === dep) inst.selectedPeople.delete(uid);
            toExclude.forEach(p => inst.excluded.set(Number(p.peoUid), { depNo: Number(p.depNo), name: p.name, depName: p.depName }));
            afterChange(inst);
            if (inst.dt) syncPageChecks(inst);
        } catch (e) {
            console.error('[OrgPeoplePicker] convert to dept failed', e);
        } finally {
            inst.convertingDeps.delete(dep);
        }
    }

    async function fetchDeptKept(inst, dep, excludeUids) {
        const res = await fetch(inst.baseUrl + '/OrgPeople/DeptKept', {
            method: 'POST', credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json', 'Accept': 'application/json' },
            body: JSON.stringify({ depNo: dep, ptyNo: inst.ptyNo, peopleStatus: inst.peopleStatus, showSelf: inst.showSelf, nodeType: inst.nodeType, exclude: excludeUids })
        });
        if (!res.ok) throw new Error('HTTP ' + res.status);
        return await res.json();
    }

    function applyRow(inst, data, checked) {
        const uid = data.peoUid, dep = data.depNo;

        // 單選：先清空再設定，永遠只留一人
        if (!inst.multiple) {
            inst.selectedDepts.clear();
            inst.excluded.clear();
            inst.selectedPeople.clear();
            if (checked) inst.selectedPeople.set(uid, { name: data.name, proName: data.proName, depNo: dep, depName: data.depName });
            return;
        }

        if (inst.selectedDepts.has(dep)) {
            if (checked) inst.excluded.delete(uid);
            else inst.excluded.set(uid, { depNo: dep, name: data.name, depName: data.depName });
        } else {
            if (checked) inst.selectedPeople.set(uid, { name: data.name, proName: data.proName, depNo: dep, depName: data.depName });
            else inst.selectedPeople.delete(uid);
        }
    }

    function syncPageChecks(inst) {
        if (!inst.dt) return;
        const cid = inst.cid;
        $('#' + cid + '_grid tbody tr').each(function () {
            const data = inst.dt.row(this).data();
            if (!data) return;
            const on = isRowSelected(inst, data.peoUid, data.depNo);
            const cb = this.querySelector('.op-row-check');
            if (cb) cb.checked = on;
            this.classList.toggle('op-row-selected', on);
        });
        updateDeptAll(inst);
    }

    // 「全選」勾選框狀態：反映目前部門是否整批選取（部分選 → indeterminate）
    function updateDeptAll(inst) {
        const el = $id(inst.cid, 'deptAll');
        if (!el) return;
        const dep = inst.currentDept;
        if (!dep || inst.global) { el.checked = false; el.indeterminate = false; el.disabled = !!inst.global; return; }
        el.disabled = false;
        const full = inst.selectedDepts.has(dep);
        let hasExcl = false; for (const [, info] of inst.excluded) if (info.depNo === dep) { hasExcl = true; break; }
        let hasIndiv = false; for (const [, info] of inst.selectedPeople) if (info.depNo === dep) { hasIndiv = true; break; }
        if (full && !hasExcl) { el.checked = true; el.indeterminate = false; }
        else if ((full && hasExcl) || hasIndiv) { el.checked = false; el.indeterminate = true; }
        else { el.checked = false; el.indeterminate = false; }
    }

    // ── 共用更新 ─────────────────────────────────────
    function afterChange(inst) {
        refreshTreeChecks(inst);
        renderChips(inst);
        updateCount(inst);
        updateDeptAll(inst);
    }

    // 樹已無勾選框：改在有選取的部門節點上加標示（名稱變色）
    function refreshTreeChecks(inst) {
        const root = $id(inst.cid, 'treeRoot');
        if (!root) return;
        root.querySelectorAll('li').forEach(li => {
            const dep = parseInt(li.dataset.dep, 10);
            const row = li.querySelector(':scope > .op-node-row');
            if (!row) return;
            const full = inst.selectedDepts.has(dep);
            let hasIndiv = false; for (const [, info] of inst.selectedPeople) if (info.depNo === dep) { hasIndiv = true; break; }
            row.classList.toggle('op-node-has-sel', full || hasIndiv);
        });
    }

    // 部門在職總數（樹載入後以 tree.countMap 為準；回顯樹未載入時用 preCount fallback）
    function deptTotal(inst, dep) {
        return (inst.tree && inst.tree.countMap[dep] != null) ? inst.tree.countMap[dep] : (inst.preCount[dep] || 0);
    }
    // 已選數 = 總數 − 排除數
    function deptNetCount(inst, dep) {
        let excl = 0; for (const [, info] of inst.excluded) if (info.depNo === dep) excl++;
        return Math.max(0, deptTotal(inst, dep) - excl);
    }

    function totalCount(inst) {
        let n = inst.selectedPeople.size;
        inst.selectedDepts.forEach(dep => { n += deptNetCount(inst, dep); });
        return n;
    }

    function updateCount(inst) {
        const el = $id(inst.cid, 'count');
        if (el) el.textContent = totalCount(inst);
    }

    function collectItems(inst) {
        const items = [];
        inst.selectedDepts.forEach(dep => {
            const node = inst.tree && inst.tree.nodeMap[dep];
            const nm = node ? node.name : ('部門' + dep);
            const total = deptTotal(inst, dep);
            const sel = deptNetCount(inst, dep);
            // 有排除時顯示「已選/總數」，讓「筆數減少」看得出是部分選取
            const label = (total && sel < total)
                ? (nm + ' 全部 (' + sel + '/' + total + ')')
                : (nm + ' 全部' + (total ? (' (' + total + ')') : ''));
            items.push({ type: 'dept', id: dep, label: label });
        });
        // 部門全選下被排除的人 → 顯示成可移除的「排除」chip（× 即還原回全選）
        inst.excluded.forEach((info, uid) => {
            if (!inst.selectedDepts.has(info.depNo)) return;
            items.push({ type: 'excluded', id: uid, label: '排除 ' + (info.depName ? info.depName + '-' : '') + info.name });
        });
        inst.selectedPeople.forEach((info, uid) =>
            items.push({ type: 'people', id: uid, label: (info.depName ? info.depName + '-' : '') + info.name }));
        return items;
    }

    function chipClass(prefix, type) {
        if (type === 'dept') return prefix + ' op-chip-dept';
        if (type === 'excluded') return prefix + ' op-chip-excluded';
        return prefix;
    }

    function renderChips(inst) {
        const box = $id(inst.cid, 'chips');
        if (!box) return;
        box.innerHTML = '';
        const items = collectItems(inst);

        if (items.length === 0) { box.innerHTML = '<span class="op-selected-empty">尚未選擇</span>'; return; }

        items.forEach(it => {
            const chip = document.createElement('span');
            chip.className = chipClass('op-chip', it.type);
            chip.appendChild(document.createTextNode(it.label + ' '));
            const b = document.createElement('button');
            b.type = 'button'; b.className = 'op-chip-x'; b.innerHTML = '&times;';
            b.addEventListener('click', () => removeChip(inst, it.type, it.id));
            chip.appendChild(b);
            box.appendChild(chip);
        });
    }

    function removeChip(inst, type, id) {
        if (type === 'dept') { toggleDept(inst, id, false); return; }  // 移除整個部門全選
        if (type === 'excluded') inst.excluded.delete(id);            // 還原被排除的人回全選
        else inst.selectedPeople.delete(id);                          // 移除個別選取
        afterChange(inst);
        if (inst.dt) syncPageChecks(inst);
    }

    function clearAll(inst) {
        inst.selectedDepts.clear();
        inst.excluded.clear();
        inst.selectedPeople.clear();
        afterChange(inst);
        if (inst.dt) syncPageChecks(inst);
    }

    function buildTokens(inst) {
        const tokens = [];
        inst.selectedDepts.forEach(dep => tokens.push('d:' + dep));
        inst.excluded.forEach((info, uid) => tokens.push('x:' + uid));
        inst.selectedPeople.forEach((info, uid) => tokens.push(String(uid)));
        return tokens.join(',');
    }

    function confirmSel(inst) {
        const hidden = $id(inst.cid, 'ids');
        if (hidden) hidden.value = buildTokens(inst);
        inst._committed = true;
        renderMainChips(inst);
        const modal = bootstrap.Modal.getInstance($id(inst.cid, 'modal'));
        if (modal) modal.hide();
    }

    // 主頁已選區（確定後呈現，可逐筆 × 移除）
    function renderMainChips(inst) {
        const wrap = $id(inst.cid, 'wrapper');
        const trigger = $id(inst.cid, 'trigger');
        const emptyEl = $id(inst.cid, 'empty');
        if (!wrap || !trigger) return;
        wrap.querySelectorAll('.op-main-chip').forEach(el => el.remove());

        const items = collectItems(inst);
        if (items.length === 0) {
            if (emptyEl) { emptyEl.style.display = ''; emptyEl.textContent = '尚未選擇人員'; }
            return;
        }
        if (emptyEl) emptyEl.style.display = 'none';

        items.forEach(it => {
            const chip = document.createElement('span');
            chip.className = chipClass('op-main-chip', it.type);
            chip.appendChild(document.createTextNode(it.label + ' '));
            const b = document.createElement('button');
            b.type = 'button'; b.className = 'op-chip-x'; b.innerHTML = '&times;';
            b.addEventListener('click', () => removeMainChip(inst, it.type, it.id));
            chip.appendChild(b);
            wrap.insertBefore(chip, trigger);
        });
    }

    function removeMainChip(inst, type, id) {
        removeChip(inst, type, id);                 // 沿用 modal 的移除邏輯（含「排除」還原）
        const hidden = $id(inst.cid, 'ids');
        if (hidden) hidden.value = buildTokens(inst);
        renderMainChips(inst);
        inst._snap = snapshot(inst);                // 已提交的移除，更新快照避免重開取消時復活
    }

    function registerApi(inst) {
        window[inst.cid + '_getTokens'] = () => buildTokens(inst);
        window[inst.cid + '_open'] = () => openModal(inst);
        window[inst.cid + '_clear'] = () => { clearAll(inst); confirmSel(inst); };
    }

    return { init: init };
})();
