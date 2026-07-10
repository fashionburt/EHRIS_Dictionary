using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Models.Common
{
    public class DepartmentTree
    {
        public int Id { get; set; }
        public string Parent { get; set; }
        public string Text { get; set; }
    }
    public class UnitTree
    {
        public string Id { get; set; }
        public string Parent { get; set; }
        public string Text { get; set; }
        public int TreeOrder { get; set; }
    }

}
