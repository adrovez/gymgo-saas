using System.Security.Claims;
using GymGo.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace GymGo.Infrastructure.Multitenancy;

/// <summary>
/// Resuelve el tenant actual:
/// 1. Si el usuario está autenticado: del claim "tenant_id" del JWT.
/// 2. Si no: del header "X-Tenant-Id" (útil para login y endpoints públicos).
/// 3. Si tampoco: null (sin tenant — sólo PlatformAdmin o endpoints anónimos
///    sin contexto de tenant).
/// </summary>
public sealed class CurrentTenantService : ICurrentTenant
{
    private readonly IHttpContextAccessor _accessor;
    private readonly Lazy<Guid?> _tenantId;

    public CurrentTenantService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
        _tenantId = new Lazy<Guid?>(Resolve);
    }

    public Guid? TenantId => _tenantId.Value;
    public bool HasTenant => _tenantId.Value.HasValue;

    private Guid? Resolve()
    {
        var ctx = _accessor.HttpContext;
        if (ctx is null) return null;

        var claim = ctx.User?.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(claim, out var fromClaim) && fromClaim != Guid.Empty)
            return fromClaim;

        if (ctx.Request.Headers.TryGetValue("X-Tenant-Id", out var header) &&
            Guid.TryParse(header.ToString(), out var fromHeader) && fromHeader != Guid.Empty)
            return fromHeader;

        return null;
    }
}
