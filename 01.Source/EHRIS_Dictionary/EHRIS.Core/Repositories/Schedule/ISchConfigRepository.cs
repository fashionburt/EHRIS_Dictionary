using EHRIS.Core.Models;
using EHRIS.Core.Models.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EHRIS.Core.Models.Schedule.SchConfigViewModel;

namespace EHRIS.Core.Repositories.Schedule
{
    public interface ISchConfigRepository
    {
        Task<List<SchConfigViewModel>> LoadSystemConfig(string scc_name);
    }
}
