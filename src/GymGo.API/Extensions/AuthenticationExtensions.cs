using System.Text;
using GymGo.Domain.Users;
using GymGo.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace GymGo.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings no está configurado en appsettings.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // dev/local; en prod ponelo en true
                options.SaveToken = true;
                // Deshabilitar el remapeo automático de claims JWT a tipos WS-Federation.
                // Sin esto, el claim "role" se convierte en ClaimTypes.Role (URL larga)
                // y RequireClaim("role", ...) nunca lo encuentra → 403 inesperado.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        // ── Políticas de autorización por rol ────────────────────────────────
        // El claim "role" es emitido por JwtTokenGenerator con el nombre del enum.
        // Member queda fuera de todas las políticas → recibe 403 en cualquier endpoint.
        services.AddAuthorization(options =>
        {
            // Solo dueño del gimnasio y soporte interno de la plataforma.
            options.AddPolicy(AuthPolicies.AdminOrOwner, p =>
                p.RequireClaim("role",
                    nameof(UserRole.PlatformAdmin),
                    nameof(UserRole.GymOwner)));

            // Administración general: propietario, staff administrativo y soporte.
            options.AddPolicy(AuthPolicies.StaffOrAbove, p =>
                p.RequireClaim("role",
                    nameof(UserRole.PlatformAdmin),
                    nameof(UserRole.GymOwner),
                    nameof(UserRole.GymStaff)));

            // Operaciones que también puede realizar un instructor/entrenador.
            options.AddPolicy(AuthPolicies.InstructorOrAbove, p =>
                p.RequireClaim("role",
                    nameof(UserRole.PlatformAdmin),
                    nameof(UserRole.GymOwner),
                    nameof(UserRole.GymStaff),
                    nameof(UserRole.Instructor)));
        });

        return services;
    }
}
