using EHRIS.Core.Models.Schedule;
using EHRIS.Core.Repositories;
using EHRIS.Core.Repositories.Schedule;
using EHRIS.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Services.Services.Schedule
{
    public class SchConfigService : ISchConfigService
    {
        private readonly ISchConfigRepository _mailQueConfigRepository; 
        private readonly ICommonService _commonService;

        public SchConfigService(ISchConfigRepository mailQueConfigRepository, ICommonService commonService)
        {
            _mailQueConfigRepository = mailQueConfigRepository;
            _commonService = commonService;
        }

        public Task<List<SchConfigViewModel>> LoadSystemInfo(string scc_name)
        {
            return _mailQueConfigRepository.LoadSystemConfig(scc_name); 
        }
         
    }
}
