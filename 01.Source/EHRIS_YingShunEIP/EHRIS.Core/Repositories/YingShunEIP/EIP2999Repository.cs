using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.YingShunEIP;
using EHRIS.Core.Models.Event;
using Microsoft.EntityFrameworkCore;

namespace EHRIS.Core.Repositories.YingShunEIP;

public class EIP2999Repository : BaseRepository, IEIP2999Repository
{
    public EIP2999Repository(ApplicationDbContext context) : base(context)
    {

    }
}