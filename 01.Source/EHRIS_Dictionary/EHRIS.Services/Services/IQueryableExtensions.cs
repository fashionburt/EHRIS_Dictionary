using System.Linq.Expressions;

namespace EHRIS.Services.Services
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> OrderByDynamic<T>(this IQueryable<T> query, string propertyName, bool asc)
        {
            var t = typeof(T);
            var prop = t.GetProperty(propertyName);
            if (prop == null) return query;

            var param = Expression.Parameter(t, "p");
            var body = Expression.MakeMemberAccess(param, prop);
            var key = Expression.Lambda(body, param);

            var method = asc ? "OrderBy" : "OrderByDescending";
            var call = Expression.Call(typeof(Queryable), method,
                new[] { t, prop.PropertyType }, query.Expression, Expression.Quote(key));

            return query.Provider.CreateQuery<T>(call);
        }
    }
}
