// OrgSelector 共用元件前端邏輯
// 對外 API：window.OrgSelector.init({...})
// 每個元件初始化後會註冊：
//   window.{cid}_getNodes()        → [{id, name}, ...]
//   window.{cid}_setSelected(ids)  → 從外部設定（async，會自動載入樹）
//   window.{cid}_clear()
//   window.{cid}_open()            → 程式化開啟 Modal
if (!window.OrgSelector) window.OrgSelector = (function () {
    'use strict';
    const instances = {};

    function init(cfg) {
        if (instances[cfg.cid]) return;

        const inst = {
            cid:        cfg.cid,
            fieldName:  cfg.fieldName,
            scope:      cfg.scope || 'Authorized',
            multiple:   cfg.multiple !== false,
            includeChildrenDefault: cfg.includeChildrenDefault !== false,
            baseUrl:    cfg.baseUrl || '',

            tree:       null,         // { nodes, nodeMap, childrenMap }
            selected:   new Map(),    // 主頁已確認選取 id → name
            modalSelected: new Map(), // Modal 暫存 id → name
            includeChildren: cfg.includeChildrenDefault !== false,
            loaded:     false
        };

        (cfg.preloaded || []).forEach(p => inst.selected.set(String(p.id), p.name));
        instances[cfg.cid] = inst;

        bindEvents(inst);
        registerPublicApi(inst);
    }

    function $cid(cid, suffix) { return document.getElementById(cid + '_' + suffix); }

    function bindEvents(inst) {
        const cid = inst.cid;

        $cid(cid, 'trigger').addEventListener('click', () => openModal(inst));
        $cid(cid, 'confirm').addEventListener('click', () => confirmSelection(inst));
        $cid(cid, 'clear').addEventListener('click', () => {
            inst.modalSelected.clear();
            applyTreeChecks(inst);
            refreshTreeState(inst);
        });

        const inc = $cid(cid, 'includeChildren');
        if (inc) inc.addEventListener('change', e => {
            inst.includeChildren = e.target.checked;
            refreshTreeState(inst);
        });

        const search = $cid(cid, 'search');
        if (search) search.addEventListener('input', e => applySearch(inst, e.target.value.trim().toLowerCase()));

        const exAll = $cid(cid, 'expandAll');
        if (exAll) exAll.addEventListener('click', () => {
            $cid(cid, 'treeRoot').querySelectorAll('li').forEach(li => li.classList.remove('collapsed'));
        });
        const colAll = $cid(cid, 'collapseAll');
        if (colAll) colAll.addEventListener('click', () => {
            $cid(cid, 'treeRoot').querySelectorAll('li.has-children').forEach(li => li.classList.add('collapsed'));
        });

        // wrapper 上的 chip × 移除
        $cid(cid, 'wrapper').addEventListener('click', e => {
            const x = e.target.closest('.org-chip-remove');
            if (!x) return;
            const chip = x.closest('.org-chip');
            inst.selected.delete(chip.dataset.id);
            renderChips(inst);
        });

        // Modal 內「已選項目」面板的 chip × 移除（同步取消樹上勾選）
        const selBox = $cid(cid, 'selectedChips');
        if (selBox) selBox.addEventListener('click', e => {
            const x = e.target.closest('.org-chip-remove');
            if (!x) return;
            const chip = x.closest('.org-chip');
            removeModalSelected(inst, chip.dataset.id);
        });

        // 「已選項目」標題列點擊收合/展開
        const selToggle = $cid(cid, 'selectedToggle');
        if (selToggle) {
            const toggleSel = () => {
                const panel = $cid(cid, 'selectedPanel');
                const collapsed = panel.classList.toggle('collapsed');
                selToggle.setAttribute('aria-expanded', collapsed ? 'false' : 'true');
            };
            selToggle.addEventListener('click', toggleSel);
            selToggle.addEventListener('keydown', e => {
                if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); toggleSel(); }
            });

            // 手機版預設收合已選面板（之後使用者仍可自行展開）
            if (window.matchMedia && window.matchMedia('(max-width: 576px)').matches) {
                const panel = $cid(cid, 'selectedPanel');
                if (panel) panel.classList.add('collapsed');
                selToggle.setAttribute('aria-expanded', 'false');
            }
        }

        // 樹節點事件委派
        const root = $cid(cid, 'treeRoot');
        root.addEventListener('click', e => {
            const toggle = e.target.closest('.org-node-toggle');
            if (toggle) {
                e.stopPropagation();
                toggle.closest('li').classList.toggle('collapsed');
                return;
            }
            if (e.target.matches('.org-node-check')) return;
            const row = e.target.closest('.org-node-row');
            if (!row) return;
            const cb = row.querySelector('.org-node-check');
            cb.checked = !cb.checked;
            cb.dispatchEvent(new Event('change', { bubbles: true }));
        });
        root.addEventListener('change', e => {
            if (!e.target.matches('.org-node-check')) return;
            handleCheckChange(inst, e.target);
        });
    }

    function openModal(inst) {
        inst.modalSelected = new Map(inst.selected);
        const inc = $cid(inst.cid, 'includeChildren');
        if (inc) inst.includeChildren = inc.checked;
        const search = $cid(inst.cid, 'search');
        if (search) search.value = '';

        const after = () => {
            applySearch(inst, '');
            applyTreeChecks(inst);
            refreshTreeState(inst);
        };

        if (!inst.loaded) {
            loadTree(inst).then(() => { renderTree(inst); after(); });
        } else {
            after();
        }

        const modal = bootstrap.Modal.getOrCreateInstance($cid(inst.cid, 'modal'));
        modal.show();
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
            console.error('[OrgSelector] load tree failed', err);
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

    function isGroup(inst, id) {
        const n = inst.tree && inst.tree.nodeMap[id];
        return !!(n && n.isGroup);
    }

    function renderTree(inst) {
        const root = $cid(inst.cid, 'treeRoot');
        root.innerHTML = '';
        const tops = inst.tree.childrenMap['root'] || [];
        tops.forEach(n => root.appendChild(renderNode(inst, n)));
    }

    function renderNode(inst, node) {
        const li = document.createElement('li');
        li.dataset.id = node.id;
        li.dataset.parentId = node.parentId == null ? '' : node.parentId;
        const children = inst.tree.childrenMap[node.id] || [];
        if (children.length) li.classList.add('has-children');
        if (node.isGroup) li.classList.add('is-group');

        const row = document.createElement('div');
        row.className = 'org-node-row';
        row.innerHTML =
            '<i class="org-node-toggle fas fa-chevron-down"></i>' +
            '<input type="checkbox" class="org-node-check" />' +
            '<i class="org-node-icon fas"></i>' +
            '<span class="org-node-name"></span>' +
            '<span class="org-node-count"></span>';
        row.querySelector('.org-node-name').textContent = node.name;
        li.appendChild(row);

        if (children.length) {
            const ul = document.createElement('ul');
            children.forEach(c => ul.appendChild(renderNode(inst, c)));
            li.appendChild(ul);
        }
        return li;
    }

    function getDescendantIds(inst, id, list) {
        list = list || [];
        (inst.tree.childrenMap[id] || []).forEach(c => {
            list.push(c.id);
            getDescendantIds(inst, c.id, list);
        });
        return list;
    }

    function handleCheckChange(inst, cb) {
        const li = cb.closest('li');
        const id = li.dataset.id;
        const checked = cb.checked;
        const node = inst.tree.nodeMap[id];

        if (!inst.multiple && checked) {
            inst.modalSelected.clear();
            $cid(inst.cid, 'treeRoot').querySelectorAll('.org-node-check').forEach(c => {
                if (c !== cb) { c.checked = false; c.indeterminate = false; }
            });
        }

        if (checked) inst.modalSelected.set(id, node.name);
        else inst.modalSelected.delete(id);

        if (inst.includeChildren && inst.multiple) {
            getDescendantIds(inst, id).forEach(did => {
                if (checked) inst.modalSelected.set(did, inst.tree.nodeMap[did].name);
                else inst.modalSelected.delete(did);
                const childCb = findCheckbox(inst, did);
                if (childCb) { childCb.checked = checked; childCb.indeterminate = false; }
            });
            bubbleParent(inst, li);
        }

        refreshTreeState(inst);
    }

    function findCheckbox(inst, id) {
        const root = $cid(inst.cid, 'treeRoot');
        const safe = (window.CSS && CSS.escape) ? CSS.escape(id) : String(id).replace(/"/g, '\\"');
        const li = root.querySelector('li[data-id="' + safe + '"]');
        return li ? li.querySelector(':scope > .org-node-row > .org-node-check') : null;
    }

    function bubbleParent(inst, li) {
        const parent = li.parentElement && li.parentElement.closest('li');
        if (!parent) return;
        const pid = parent.dataset.id;
        const descendants = getDescendantIds(inst, pid);
        const allChecked = descendants.length > 0 && descendants.every(d => inst.modalSelected.has(d));
        const parentCb = parent.querySelector(':scope > .org-node-row > .org-node-check');
        if (allChecked) {
            inst.modalSelected.set(pid, inst.tree.nodeMap[pid].name);
            if (parentCb) parentCb.checked = true;
        } else {
            inst.modalSelected.delete(pid);
            if (parentCb) parentCb.checked = false;
        }
        bubbleParent(inst, parent);
    }

    function applyTreeChecks(inst) {
        $cid(inst.cid, 'treeRoot').querySelectorAll('.org-node-check').forEach(cb => {
            const id = cb.closest('li').dataset.id;
            cb.checked = inst.modalSelected.has(id);
            cb.indeterminate = false;
        });
    }

    function refreshTreeState(inst) {
        // 更新父節點 indeterminate + 「已勾選數」徽章
        $cid(inst.cid, 'treeRoot').querySelectorAll('li.has-children').forEach(li => {
            const id = li.dataset.id;
            const descendants = getDescendantIds(inst, id);
            const sel = descendants.filter(d => inst.modalSelected.has(d)).length;
            const cb = li.querySelector(':scope > .org-node-row > .org-node-check');
            const badge = li.querySelector(':scope > .org-node-row > .org-node-count');

            if (cb) {
                if (inst.includeChildren) {
                    cb.indeterminate = (sel > 0 && sel < descendants.length);
                } else {
                    cb.indeterminate = false;
                }
            }
            if (badge) {
                if (sel > 0) {
                    badge.textContent = sel;
                    badge.classList.add('has-selected');
                } else {
                    badge.classList.remove('has-selected');
                }
            }
        });
        updateCount(inst);
        renderModalSelected(inst);
    }

    // Modal 下方「已選項目」清單（呈現 modalSelected，排除機關虛擬節點）
    function renderModalSelected(inst) {
        const box = $cid(inst.cid, 'selectedChips');
        if (!box) return;
        box.innerHTML = '';

        const entries = [];
        inst.modalSelected.forEach((name, id) => {
            if (!isGroup(inst, id)) entries.push([id, name]);
        });

        const headCount = $cid(inst.cid, 'selectedHeadCount');
        if (headCount) headCount.textContent = entries.length ? entries.length : '';

        const panel = $cid(inst.cid, 'selectedPanel');
        if (entries.length === 0) {
            if (panel) panel.classList.add('is-empty');
            const empty = document.createElement('span');
            empty.className = 'org-selected-empty';
            empty.textContent = '尚未選擇';
            box.appendChild(empty);
            return;
        }
        if (panel) panel.classList.remove('is-empty');

        entries.forEach(([id, name]) => {
            const chip = document.createElement('span');
            chip.className = 'org-chip';
            chip.dataset.id = id;
            chip.appendChild(document.createTextNode(name + ' '));
            const btn = document.createElement('button');
            btn.type = 'button';
            btn.className = 'org-chip-remove';
            btn.setAttribute('aria-label', '移除');
            btn.innerHTML = '&times;';
            chip.appendChild(btn);
            box.appendChild(chip);
        });
    }

    // 從「已選項目」面板移除一筆，並同步取消樹上的勾選（含子節點連動）
    function removeModalSelected(inst, id) {
        const cb = findCheckbox(inst, id);
        if (cb) {
            cb.checked = false;
            handleCheckChange(inst, cb);
        } else {
            inst.modalSelected.delete(id);
            refreshTreeState(inst);
        }
    }

    function realCount(inst, map) {
        // 排除機關虛擬節點
        let n = 0;
        map.forEach((_, id) => { if (!isGroup(inst, id)) n++; });
        return n;
    }

    function updateCount(inst) {
        const el = $cid(inst.cid, 'count');
        if (el) el.textContent = realCount(inst, inst.modalSelected);
    }

    function applySearch(inst, q) {
        if (!inst.tree) return;
        const root = $cid(inst.cid, 'treeRoot');
        const empty = $cid(inst.cid, 'empty');

        // reset
        root.querySelectorAll('li').forEach(li => {
            li.style.display = '';
            const row = li.querySelector(':scope > .org-node-row');
            row.classList.remove('is-matched');
            const span = row.querySelector('.org-node-name');
            span.textContent = inst.tree.nodeMap[li.dataset.id].name;
        });
        if (!q) { if (empty) empty.style.display = 'none'; return; }

        const matches = new Set();
        inst.tree.nodes.forEach(n => {
            if (n.name.toLowerCase().indexOf(q) !== -1) {
                matches.add(n.id);
                let p = n.parentId;
                while (p) {
                    matches.add(p);
                    p = inst.tree.nodeMap[p] ? inst.tree.nodeMap[p].parentId : null;
                }
            }
        });

        let visibleMatches = 0;
        root.querySelectorAll('li').forEach(li => {
            const id = li.dataset.id;
            if (!matches.has(id)) { li.style.display = 'none'; return; }
            li.classList.remove('collapsed');
            const name = inst.tree.nodeMap[id].name;
            const lower = name.toLowerCase();
            const idx = lower.indexOf(q);
            if (idx !== -1) {
                visibleMatches++;
                const row = li.querySelector(':scope > .org-node-row');
                row.classList.add('is-matched');
                const span = row.querySelector('.org-node-name');
                span.textContent = '';
                span.append(name.slice(0, idx));
                const mark = document.createElement('mark');
                mark.textContent = name.slice(idx, idx + q.length);
                span.appendChild(mark);
                span.append(name.slice(idx + q.length));
            }
        });
        if (empty) empty.style.display = visibleMatches === 0 ? '' : 'none';
    }

    function confirmSelection(inst) {
        inst.selected = new Map(inst.modalSelected);
        renderChips(inst);
        const modal = bootstrap.Modal.getInstance($cid(inst.cid, 'modal'));
        if (modal) modal.hide();
    }

    function renderChips(inst) {
        const wrapper = $cid(inst.cid, 'wrapper');
        const trigger = $cid(inst.cid, 'trigger');
        const hidden  = $cid(inst.cid, 'ids');

        wrapper.querySelectorAll('.org-chip, .org-selector-empty').forEach(el => el.remove());

        // 只顯示真實部門（過濾機關虛擬節點），同時 hidden input 也只寫入真實 dep_no
        const realIds = [];
        inst.selected.forEach((name, id) => {
            if (isGroup(inst, id)) return;
            realIds.push(id);
        });

        if (realIds.length === 0) {
            const empty = document.createElement('span');
            empty.className = 'org-selector-empty';
            empty.textContent = '尚未選擇單位';
            wrapper.insertBefore(empty, trigger);
        } else {
            realIds.forEach(id => {
                const chip = document.createElement('span');
                chip.className = 'org-chip';
                chip.dataset.id = id;
                chip.appendChild(document.createTextNode(inst.selected.get(id) + ' '));
                const btn = document.createElement('button');
                btn.type = 'button';
                btn.className = 'org-chip-remove';
                btn.setAttribute('aria-label', '移除');
                btn.innerHTML = '&times;';
                chip.appendChild(btn);
                wrapper.insertBefore(chip, trigger);
            });
        }

        hidden.value = realIds.join(',');
    }

    function registerPublicApi(inst) {
        window[inst.cid + '_getNodes'] = function () {
            return Array.from(inst.selected.entries())
                .filter(e => !isGroup(inst, e[0]))
                .map(e => ({ id: e[0], name: e[1] }));
        };
        window[inst.cid + '_setSelected'] = async function (ids) {
            if (!inst.loaded) await loadTree(inst);
            const arr = Array.isArray(ids) ? ids : String(ids || '').split(',');
            inst.selected.clear();
            arr.forEach(id => {
                id = String(id).trim();
                const n = inst.tree.nodeMap[id];
                if (id && n && !n.isGroup) {
                    inst.selected.set(id, n.name);
                }
            });
            renderChips(inst);
        };
        window[inst.cid + '_clear'] = function () {
            inst.selected.clear();
            renderChips(inst);
        };
        window[inst.cid + '_open'] = function () { openModal(inst); };
    }

    return { init: init };
})();
