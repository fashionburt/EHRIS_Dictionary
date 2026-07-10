using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Services.Common;

public class ExeEventOperator : EventOperatorRepository
{
    public ExeEventOperator(IEventObjectRepository repository, int uid, int sfuNo, string fromIP, EventMode mode)
        : base(repository, uid, sfuNo, mode,fromIP, WhoExec.Schedule) { }

   
}
