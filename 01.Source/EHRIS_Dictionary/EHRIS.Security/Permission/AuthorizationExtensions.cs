using Microsoft.Extensions.DependencyInjection;

namespace EHRIS.Security.Permission;
public static class AuthorizationExtensions
{
    public static IServiceCollection AddMyAppAuthorization(this IServiceCollection services)
    {
        //services.AddAuthorization(options =>
        //{
        //    options.AddPolicy("AdminOnly", policy =>
        //        policy.RequireRole("Admin"));
        //});
        services.AddScoped<IPermissionService, PermissionService>();
        return services;
    }
}

