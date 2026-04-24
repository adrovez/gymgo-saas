using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GymGo.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace GymGo.Infrastructure.Identity;

public sealed class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid? UserId
    {
        get
        {
            var sub = _accessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? _accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email =>
        _accessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Email)?.Value
        ?? _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public string? Role =>
        _accessor.HttpContext?.User?.FindFirst("role")?.Value
        ?? _accessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

    public bool IsAuthenticated =>
        _accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
