using EHRIS.Core.DbContext;
using EHRIS.Core.Entities;
using EHRIS.Core.Models.Common;
using EHRIS.Core.Models.YingShunEIP;
using EHRIS.Core.Models.Event;
using Microsoft.EntityFrameworkCore;

namespace EHRIS.Core.Repositories.YingShunEIP;

public class EIP2997Repository : BaseRepository, IEIP2997Repository 
{
    public EIP2997Repository(ApplicationDbContext context) : base(context)
    {

    }
}