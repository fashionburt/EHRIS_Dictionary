using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Repositories;

public class LoadSystemConfigRepository:BaseRepository, ILoadSystemConfigRepository
{
    private readonly IDbHealthCheck _dbHealthCheck;
    public LoadSystemConfigRepository(ApplicationDbContext context, IDbHealthCheck dbHealthCheck) : base(context)
    {
        _dbHealthCheck = dbHealthCheck;
    }

    
    public async Task LoadSystemConfig(string MasterKey)
    {
        AppConfig.LoadSystemInfo(_context, _dbHealthCheck, MasterKey);

    }


}
