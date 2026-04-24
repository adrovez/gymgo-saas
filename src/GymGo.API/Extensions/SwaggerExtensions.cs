using Microsoft.OpenApi.Models;

namespace GymGo.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(opts =>
        {
            opts.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "GymGo API",
                Version = "v1",
                Description = "API multi-tenant para gestión de gimnasios."
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "JWT Bearer token. Ej: Bearer eyJhbGciOi...",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
            };
            opts.AddSecurityDefinition("Bearer", securityScheme);
            opts.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, Array.Empty<string>() }
            });
        });

        return services;
    }
}
