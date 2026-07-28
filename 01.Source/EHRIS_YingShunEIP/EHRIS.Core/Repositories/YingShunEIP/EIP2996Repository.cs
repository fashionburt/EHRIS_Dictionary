using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.YingShunEIP;
using EHRIS.Core.Models.Event;
using Microsoft.EntityFrameworkCore;

namespace EHRIS.Core.Repositories.YingShunEIP;

public class EIP2996Repository : BaseRepository, IEIP2996Repository
{
    public EIP2996Repository(ApplicationDbContext context) : base(context)
    {

    }
}