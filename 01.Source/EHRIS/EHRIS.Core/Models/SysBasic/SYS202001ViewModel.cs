using EHRIS.Core.Models.Common;

namespace EHRIS.Core.Models.SysBasic
{
    public class SYS202001ViewModel
    {
    }

    public class SYS202001EDTViewModel
    {
        public int deptId { get; set; }
        public string deptCode { get; set; }
        public string deptName { get; set; }
        public int? deptOrder { get; set; }
        public int deptLevel { get; set; }
        public int parentId { get; set; }
        public string parentDeptName { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanInsert { get; set; }

    }

    public class DepartmentDataTableRequest
    {
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public string searchValue { get; set; }
        public List<DataTableOrder> orderby { get; set; }
        public List<DataTableColumn> columns { get; set; }
        public ExtraSearch extraSearch { get; set; }
    }

    public class DataTableOrder
    {
        public int column { get; set; }
        public string dir { get; set; }
    }

    public class DataTableColumn
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
    }

    public class DepartmentListViewModel
    {
        public int deptId { get; set; }
        public string deptCode { get; set; }
        public string deptName { get; set; }
        public int? deptOrder { get; set; }
        public string deptModifyName { get; set; }
        public DateTime? deptModifyTime { get; set; }
        public int dep_level { get; set; }
    }

    public class DepartmentUpdateModel
    {
        public int deptId { get; set; }
        public string deptCode { get; set; }
        public string deptName { get; set; }
        public int? deptOrder { get; set; }
        public int parentId { get; set; }
        public string parentDeptName { get; set; }
    }

    public class DeleteRequestModel
    {
        public int id { get; set; }
    }

}