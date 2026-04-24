using GymGo.Application.Common.Interfaces;
using GymGo.Infrastructure.Authentication;
using GymGo.Infrastructure.Identity;
using GymGo.Infrastructure.Multitenancy;
using GymGo.Infrastructure.Persistence;
using GymGo.Infrastructure.Persistence.Interceptors;
using GymGo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GymGo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // HttpContext + servicios de contexto del request
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddScoped<ICurrentTenant, CurrentTenantService>();

        // Servicios genéricos
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        // JWT
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Interceptors de EF Core
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<TenantScopeSaveChangesInterceptor>();

        // DbContext
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var auditInterceptor = sp.GetRequiredService<AuditableEntitySaveChangesInterceptor>();
            var tenantInterceptor = sp.GetRequiredService<TenantScopeSaveChangesInterceptor>();

            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

            options.AddInterceptors(auditInterceptor, tenantInterceptor);
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}
