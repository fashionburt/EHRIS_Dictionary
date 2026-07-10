function initDepartmentTree() {
    //樹狀選單初始化
    var $select = $('#departmentSelect');
    if ($select.length > 0 && $('#departmentTreeModal').find('.tree-multiselect').length === 0) {
        $select.treeMultiselect({
            searchable: true,
            searchParams: ['section', 'text'],
            allowBatchSelection: true,
            startCollapsed: false,
            sectionDelimiter: "/" // 這樣可以正確解析層級關係
        });
    }
}

initDepartmentTree();

if (!window.deptTreeObserver) {
    window.deptTreeObserver = new MutationObserver(function () {
        initDepartmentTree();
    });
    window.deptTreeObserver.observe(document.body, { childList: true, subtree: true });
}

// 監聽選取變化，只顯示 Text，不包含 Parent
$(document).off('change', '#departmentTreeModal').on('change', '#departmentTreeModal', function () {
    $(this).find(".tree-multiselect .selected").each(function () {
        let textOnly = $(this).find(".item").text(); // 取得 Text
        $(this).find(".section-name").remove(); // 移除父節點名稱
    });
});

//  Modal 按下「確定」時，把勾選值寫回 input
$(document).off('click', '#saveDepartments').on('click', '#saveDepartments', function () {
    var $select = $('#departmentSelect');
    var selectedValues = $select.val(); // 取得所有選取的值

    $('#selectedDepartments').val(selectedValues.join(','));// 顯示選取的部門代碼
    var tagContainer = $('#selectedDepartmentsTags');
    tagContainer.empty();

    selectedValues.forEach(val => {
        var label = $select.find(`option[value="${val}"]`).text();
        tagContainer.append(`
            <span class="tag badge bg-primary me-1 mb-1">
                ${label} <span class="ms-1 remove-tag" data-value="${val}" style="cursor:pointer;">×</span>
            </span>
        `);
    });
    // 關閉 Modal
    $('#departmentTreeModal').modal('hide');
});