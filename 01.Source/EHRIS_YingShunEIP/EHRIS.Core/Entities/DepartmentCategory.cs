using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EHRIS.Core.Entities
{
    [Table("department_category")]
    public class DepartmentCategory
    {
        /// <summary>
        /// depc_no
        /// </summary>
        [Key]
        [Column("depc_no")]
        public int DepcNo { get; set; }
        /// <summary>
        /// sup_no
        /// </summary>
        [Column("sup_no")]
        public int SupNo { get; set; }
        /// <summary>
        /// dep_no
        /// </summary>
        [Column("dep_no")]
        public int DepNo { get; set; }
    }

}
