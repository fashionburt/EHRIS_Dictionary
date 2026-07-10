using EHRIS.Core.DbContext;
using EHRIS.Core.Models;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Schedule;
using Microsoft.EntityFrameworkCore;
using NPOI.OpenXmlFormats.Dml.Diagram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Repositories.Schedule
{
    public class SchConfigRepository : BaseRepository, ISchConfigRepository
    {
        public SchConfigRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<List<SchConfigViewModel>> LoadSystemConfig(string scc_name)
        {
            var query = (from cfg in _context.scheduleConfig.AsNoTracking()
                         select cfg);

            if (!string.IsNullOrWhiteSpace(scc_name))
            {
                query = query.Where(x => x.SccName == scc_name);
            }

            var dataList = await query.Select(x => new SchConfigViewModel
            {
                scc_variable = x.SccVariable,
                scc_value = x.SccValue,
            }).ToListAsync();

            return dataList;
        }
         

    }
}
