using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Services.Common;

public class WebEventOperator : EventOperatorRepository
{
    public WebEventOperator(IEventObjectRepository repository, int uid, int sfuNo, EventMode mode, string fromIP)
    : base(repository, uid, sfuNo, mode,fromIP, WhoExec.Personal) { }

    
}
