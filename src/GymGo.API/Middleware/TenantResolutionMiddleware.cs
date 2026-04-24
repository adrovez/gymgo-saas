using GymGo.Application.Common.Interfaces;
using Serilog.Context;

namespace GymGo.API.Middleware;

/// <summary>
/// Hace logs por request enriquecidos con tenant_id y user_id.
/// El ICurrentTenant se materializa al primer acceso (Lazy).
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICurrentTenant tenant, ICurrentUser user)
    {
        using (LogContext.PushProperty("TenantId", tenant.TenantId?.ToString() ?? "none"))
        using (LogContext.PushProperty("UserId", user.UserId?.ToString() ?? "anonymous"))
        {
            await _next(context);
        }
    }
}
