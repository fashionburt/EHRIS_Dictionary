using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Repositories
{
    public interface ISysVariableRepository
    {
        Task<List<SysVariable>> GetSysVariableBySarVodeCodeAsync(string sarCode);

        Task<List<StaParamsViewModel>> GetStaParamsAsync(string stp_code);
    }
}
