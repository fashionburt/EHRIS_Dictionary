// OrgSelectorDropdown 共用元件 ── 下拉式樹狀（單選）
// 後端資料源與 OrgSelector modal 共用：/OrgSelector/Tree?scope=
// 對外 API：
//   window.{cid}_getNode()        → {id, name} | null
//   window.{cid}_setSelected(id)  → 從外部設定（async，會自動載入樹）
//   window.{cid}_clear()
//   window.{cid}_open()
if (!window.OrgSelectorDropdown) window.OrgSelectorDropdown = (function () {
    'use strict';
    const instances = {};

    function init(cfg) {
        if (instances[cfg.cid]) return;

        const inst = {
            cid:          cfg.cid,
            fieldName:    cfg.fieldName,
            scope:        cfg.scope || 'AuthorizedDept',
            showRoot:     cfg.showRoot !== false,
            placeholder:  cfg.placeholder || '請選擇',
            baseUrl:      cfg.baseUrl || '',

            tree:         null,    // { nodes, nodeMap, childrenMap }
            selectedId:   (cfg.preloaded && cfg.preloaded.id) ? String(cfg.preloaded.id) : '',
            selectedName: (cfg.preloaded && cfg.preloaded.name) || '',
            loaded:       false,
            open:         false
        };

        instances[cfg.cid] = inst;
        bindEvents(inst);
        registerPublicApi(inst);
    }

    function $cid(cid, s) { return document.getElementById(cid + '_' + s); }

    function bindEvents(inst) {
        const cid = inst.cid;

        $cid(cid, 'trigger').addEventListener('click', e => {
            if (e.target.closest('.org-dd-clear')) return; // 清除鈕另外處理
            toggleOpen(inst);
        });

        const clr = $cid(cid, 'clear');
        if (clr) clr.addEventListener('click', e => {
            e.stopPropagation();
            setSelection(inst, '', '');
            closeDropdown(inst);
        });

        const search = $cid(cid, 'search');
        if (search) search.addEventListener('input', e => applySearch(inst, e.target.value.trim().toLowerCase()));

        const root = $cid(cid, 'treeRoot');
        root.addEventListener('click', e => {
            const toggle = e.target.closest('.org-dd-toggle');
            if (toggle) {
                e.stopPropagation();
                toggle.closest('li').classList.toggle('collapsed');
                return;
            }
            const row = e.target.closest('.org-dd-row');
            if (!row) return;
            const li = row.closest('li');
            const node = inst.tree.nodeMap[li.dataset.id];
            if (node.isGroup) {
                // 機關（群組）節點不可選，點擊 = 展開/折疊
                if (li.classList.contains('has-children')) li.classList.toggle('collapsed');
                return;
            }
            setSelection(inst, node.id, node.name);
            closeDropdown(inst);
        });

        // 點元件外關閉
        document.addEventListener('click', e => {
            if (!inst.open) return;
            if (!e.target.closest('#' + cid + '_wrapper')) closeDropdown(inst);
        });
    }

    function toggleOpen(inst) { inst.open ? closeDropdown(inst) : openDropdown(inst); }

    function openDropdown(inst) {
        const after = () => {
            const s = $cid(inst.cid, 'search'); if (s) s.value = '';
            applySearch(inst, '');
            highlightSelected(inst);
        };
        if (!inst.loaded) {
            loadTree(inst).then(() => { renderTree(inst); after(); });
        } else {
            after();
        }
        $cid(inst.cid, 'panel').classList.add('open');
        $cid(inst.cid, 'wrapper').classList.add('open');
        $cid(inst.cid, 'trigger').setAttribute('aria-expanded', 'true');
        inst.open = true;
        const s = $cid(inst.cid, 'search'); if (s) setTimeout(() => s.focus(), 30);
    }

    function closeDropdown(inst) {
        $cid(inst.cid, 'panel').classList.remove('open');
        $cid(inst.cid, 'wrapper').classList.remove('open');
        $cid(inst.cid, 'trigger').setAttribute('aria-expanded', 'false');
        inst.open = false;
    }

    async function loadTree(inst) {
        const loading = $cid(inst.cid, 'loading');
        loading.style.display = '';
        try {
            const url = inst.baseUrl + '/OrgSelector/Tree?scope=' + encodeURIComponent(inst.scope);
            const res = await fetch(url, { credentials: 'same-origin', headers: { 'Accept': 'application/json' } });
            if (!res.ok) throw new Error('HTTP ' + res.status);
            const nodes = await res.json();
            inst.tree = buildIndex(nodes);
            inst.loaded = true;
        } catch (err) {
            console.error('[OrgSelectorDropdown] load tree failed', err);
            loading.textContent = '載入失敗';
            return;
        }
        loading.style.display = 'none';
    }

    function buildIndex(nodes) {
        const nodeMap = {};
        const childrenMap = {};
        nodes.forEach(n => {
            n.id = String(n.id);
            n.parentId = (n.parentId == null || n.parentId === '') ? null : String(n.parentId);
            n.isGroup = !!n.isGroup;
            nodeMap[n.id] = n;
            const pid = n.parentId == null ? 'root' : n.parentId;
            (childrenMap[pid] = childrenMap[pid] || []).push(n);
        });
        return { nodes, nodeMap, childrenMap };
    }

    // 取頂層節點；ShowRoot=false 時只收合「最上層、非群組、有子節點」的根節點
    // （即 dep_parentid=0 那層，主要是 All 模式的根機關），把其子節點上移為頂層。
    // 群組節點(機關層)與沒有子節點的葉(權責機關清單)維持原樣。
    function getTops(inst) {
        let tops = inst.tree.childrenMap['root'] || [];
        if (!inst.showRoot) {
            tops = tops.reduce((acc, t) => {
                const kids = inst.tree.childrenMap[t.id] || [];
                if (!t.isGroup && kids.length) return acc.concat(kids);
                return acc.concat([t]);
            }, []);
        }
        return tops;
    }

    function renderTree(inst) {
        const root = $cid(inst.cid, 'treeRoot');
        root.innerHTML = '';
        getTops(inst).forEach(n => root.appendChild(renderNode(inst, n)));
    }

    function renderNode(inst, node) {
        const li = document.createElement('li');
        li.dataset.id = node.id;
        const children = inst.tree.childrenMap[node.id] || [];
        if (children.length) li.classList.add('has-children');
        if (node.isGroup) li.classList.add('is-group');

        const row = document.createElement('div');
        row.className = 'org-dd-row';
        row.innerHTML =
            '<i class="org-dd-toggle fas fa-chevron-down"></i>' +
            '<i class="org-dd-icon fas"></i>' +
            '<span class="org-dd-name"></span>';
        row.querySelector('.org-dd-name').textContent = node.name;
        li.appendChild(row);

        if (children.length) {
            const ul = document.createElement('ul');
            children.forEach(c => ul.appendChild(renderNode(inst, c)));
            li.appendChild(ul);
        }
        return li;
    }

    function setSelection(inst, id, name) {
        inst.selectedId = id ? String(id) : '';
        inst.selectedName = name || '';

        const txt = $cid(inst.cid, 'text');
        const hid = $cid(inst.cid, 'id');
        const clr = $cid(inst.cid, 'clear');
        hid.value = inst.selectedId;

        if (inst.selectedId) {
            txt.textContent = inst.selectedName;
            txt.classList.remove('is-placeholder');
            if (clr) clr.style.display = '';
        } else {
            txt.textContent = inst.placeholder;
            txt.classList.add('is-placeholder');
            if (clr) clr.style.display = 'none';
        }
        highlightSelected(inst);
    }

    function highlightSelected(inst) {
        const root = $cid(inst.cid, 'treeRoot');
        if (!root) return;
        root.querySelectorAll('.org-dd-row.is-selected').forEach(r => r.classList.remove('is-selected'));
        if (!inst.selectedId) return;

        const safe = (window.CSS && CSS.escape) ? CSS.escape(inst.selectedId) : String(inst.selectedId).replace(/"/g, '\\"');
        const li = root.querySelector('li[data-id="' + safe + '"]');
        if (!li) return;

        const row = li.querySelector(':scope > .org-dd-row');
        if (row) row.classList.add('is-selected');
        // 展開祖先路徑
        let p = li.parentElement && li.parentElement.closest('li');
        while (p) { p.classList.remove('collapsed'); p = p.parentElement && p.parentElement.closest('li'); }
    }

    function applySearch(inst, q) {
        if (!inst.tree) return;
        const root = $cid(inst.cid, 'treeRoot');
        const empty = $cid(inst.cid, 'empty');

        // reset
        root.querySelectorAll('li').forEach(li => {
            li.style.display = '';
            const row = li.querySelector(':scope > .org-dd-row');
            row.classList.remove('is-matched');
            row.querySelector('.org-dd-name').textContent = inst.tree.nodeMap[li.dataset.id].name;
        });
        if (!q) { if (empty) empty.style.display = 'none'; return; }

        const matches = new Set();
        inst.tree.nodes.forEach(n => {
            if (n.name.toLowerCase().indexOf(q) !== -1) {
                matches.add(n.id);
                let p = n.parentId;
                while (p) { matches.add(p); p = inst.tree.nodeMap[p] ? inst.tree.nodeMap[p].parentId : null; }
            }
        });

        let visible = 0;
        root.querySelectorAll('li').forEach(li => {
            const id = li.dataset.id;
            if (!matches.has(id)) { li.style.display = 'none'; return; }
            li.classList.remove('collapsed');
            const name = inst.tree.nodeMap[id].name;
            const idx = name.toLowerCase().indexOf(q);
            if (idx !== -1) {
                visible++;
                const row = li.querySelector(':scope > .org-dd-row');
                row.classList.add('is-matched');
                const span = row.querySelector('.org-dd-name');
                span.textContent = '';
                span.append(name.slice(0, idx));
                const mark = document.createElement('mark');
                mark.textContent = name.slice(idx, idx + q.length);
                span.appendChild(mark);
                span.append(name.slice(idx + q.length));
            }
        });
        if (empty) empty.style.display = visible === 0 ? '' : 'none';
    }

    function registerPublicApi(inst) {
        window[inst.cid + '_getNode'] = function () {
            return inst.selectedId ? { id: inst.selectedId, name: inst.selectedName } : null;
        };
        window[inst.cid + '_setSelected'] = async function (id) {
            id = String(id == null ? '' : id).trim();
            if (!inst.loaded) await loadTree(inst);
            const n = id && inst.tree ? inst.tree.nodeMap[id] : null;
            if (n && !n.isGroup) setSelection(inst, n.id, n.name);
            else setSelection(inst, '', '');
        };
        window[inst.cid + '_clear'] = function () { setSelection(inst, '', ''); };
        window[inst.cid + '_open'] = function () { openDropdown(inst); };
    }

    return { init: init };
})();
