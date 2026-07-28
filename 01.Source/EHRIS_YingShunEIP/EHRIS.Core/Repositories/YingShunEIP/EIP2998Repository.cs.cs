using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.YingShunEIP;
using EHRIS.Core.Models.Event;
using Microsoft.EntityFrameworkCore;

namespace EHRIS.Core.Repositories.YingShunEIP;

public class EIP2998Repository : BaseRepository, IEIP2998Repository
{
    public EIP2998Repository(ApplicationDbContext context) : base(context)
    {

    }
}