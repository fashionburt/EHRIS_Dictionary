using EHRIS.Core.DbContext;
using EHRIS.Core.Repositories;
using NPOI.OpenXmlFormats.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Models.Schedule
{
    public  class SchConfigViewModel
    {
        public string scc_variable { get; set; }
        public string scc_value { get; set; } 
    }
}
