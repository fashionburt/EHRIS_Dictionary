using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHRIS.Core.Repositories
{
    public interface IBaseRepository
    {
        Task<LoginUserInfo> GetUserInfo(int peo_uid);
        Task<LoginUserInfo> GetUserInfoByAccount(int acc_no);
        Task<int> SaveChangesAsync(IDataLogger operateContext);
    }
}
