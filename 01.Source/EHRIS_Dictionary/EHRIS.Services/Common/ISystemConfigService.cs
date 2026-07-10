using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Services.Common
{
    public interface ISystemConfigService
    {
        void LoadSystemInfo(string MasterKey);
    }
}
