using EHRIS.Core.Models.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Models.SysBasic
{
    public class AdminDataTableRequest
    {
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public string search { get; set; }
        public List<Column> columns { get; set; }
        public List<Order> orderby { get; set; }
        public ExtraSearch extraSearch { get; set; }
    }
        public class Column
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
    }

    public class Order
    {
        public int column { get; set; }
        public string dir { get; set; }
    }
    public class AdminEmailViewModel
    {
        public int BseNo { get; set; } 
        public string BseEmailType { get; set; } 
        public string BseEmail { get; set; }
        public int BseOrder { get; set; }
    }
    public class AdminDataTableResponse<T>
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<T> data { get; set; }
    }

    public class AdminListViewModel
    {
        public int acc_no { get; set; }
        public string unit { get; set; } = "";
        public string name { get; set; } = "";
        public string loginAccount { get; set; } = "";
        public string accountStatus { get; set; } = "";
        public string modifyName { get; set; }
        public DateTime? modifyTime { get; set; }
    }

    public class AdminUpdateModel
    {
        public int acc_no { get; set; }

        [Required(ErrorMessage = "請選擇性別")]
        public bool? sex { get; set; }

        [Required(ErrorMessage = "請選擇單位")]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇有效的單位")]
        public int unitId { get; set; }

        [Required(ErrorMessage = "請選擇職稱")]
        [Range(1, int.MaxValue, ErrorMessage = "請選擇有效的職稱")]
        public int proNo { get; set; }

        [Required(ErrorMessage = "姓名為必填欄位")]
        [StringLength(50)]
        public string name { get; set; }

        [Required(ErrorMessage = "身分證字號為必填欄位")]
        [StringLength(20)]
        public string idNumber { get; set; }

        [Required(ErrorMessage = "出生日期為必填欄位")]
        [DataType(DataType.Date)]
        public DateTime birthDate { get; set; }

        [Required(ErrorMessage = "登入帳號為必填欄位")]
        [StringLength(100)]
        public string loginAccount { get; set; }

        [StringLength(20, MinimumLength = 8, ErrorMessage = "密碼長度必須介於 8 到 20 個字之間")]
        [DataType(DataType.Password)]
        public string password { get; set; }

        [BindNever] 
        public List<int> selectedRoleIds { get; set; }
        public List<int> SelectedPtypeNos { get; set; }
        public bool managesAllDepts { get; set; }
        public List<int> selectedDeptIds { get; set; }

        public List<AdminEmailViewModel> Emails { get; set; } = new List<AdminEmailViewModel>();
    }

    public class AdminDetailsViewModel
    {
        public int acc_no { get; set; }
        public int unitId { get; set; }
        public int proNo { get; set; } 
        public string name { get; set; }
        public bool sex { get; set; }
        public string loginAccount { get; set; }
        public string idNumber { get; set; }
        public DateTime birthDate { get; set; }
        public Guid BasId { get; set; }
        [NotMapped]
        public List<AdminEmailViewModel> Emails { get; set; } = new List<AdminEmailViewModel>();

        [NotMapped]
        public List<int> selectedRoleIds { get; set; } = new List<int>();

        [NotMapped]
        public List<int> SelectedPtypeNos { get; set; } = new List<int>();

        [NotMapped] 
        public bool managesAllDepts { get; set; }

        [NotMapped] 
        public List<int> selectedDeptIds { get; set; } = new List<int>();

    }

    public class SYS201001ViewModel
    {
        public List<DepartmentTree> DepartmentTrees { get; set; }
    }
    public class PasswordHistoryDto
    {
        public string Hash { get; set; }
        public string Salt { get; set; }
    }
}