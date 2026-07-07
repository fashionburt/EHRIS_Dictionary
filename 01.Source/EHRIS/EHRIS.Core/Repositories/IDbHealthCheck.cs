using EHRIS.Core.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Repositories
{
    public interface IDbHealthCheck
    {
        ServiceResult<bool> CanConnect();
    }
}
