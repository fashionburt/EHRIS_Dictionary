using EHRIS.Core.Entities;

namespace EHRIS.Core.DbContext;

public static class DbContextExtensions
{
    /// <summary>
    /// 擴充 SaveChangesAsync，傳入執行訊
    /// </summary>
    /// <param name="context">不需傳入</param>
    /// <param name="eventOperatorDto"></param>
    /// <param name="cancellationToken">不需傳入</param>
    /// <returns></returns>
    //public static Task<int> SaveChangesAsync(
    //    this ApplicationDbContext context,
    //    EventOperatorDto eventOperatorDto,
    //    CancellationToken cancellationToken = default)
    //{
    //    context._eventOperatorDto = eventOperatorDto;
    //    return context.SaveChangesAsync(cancellationToken);
    //}
}
