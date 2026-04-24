namespace GymGo.Application.Common.Interfaces;

/// <summary>
/// Resuelve el usuario actual del request. Implementado en API
/// usando IHttpContextAccessor + Claims.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}
