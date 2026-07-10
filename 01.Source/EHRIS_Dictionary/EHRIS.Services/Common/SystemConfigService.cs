using EHRIS.Core.Models.Common;
using EHRIS.Core.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Services.Common;

public  class SystemConfigService: ISystemConfigService
{
    private readonly ILoadSystemConfigRepository _systemConfigrepository;
    

    public SystemConfigService(ILoadSystemConfigRepository systemConfigrepository)
    {
        _systemConfigrepository = systemConfigrepository;
      
    }

    public void LoadSystemInfo(string MasterKey)
    {
        _systemConfigrepository.LoadSystemConfig(MasterKey);
        
    }
}
