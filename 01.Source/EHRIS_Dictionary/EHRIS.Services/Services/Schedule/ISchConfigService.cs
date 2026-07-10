using EHRIS.Core.Models.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Services.Services.Schedule
{
    public interface ISchConfigService
    {
        Task<List<SchConfigViewModel>> LoadSystemInfo(string scc_name) ;
    }
}
