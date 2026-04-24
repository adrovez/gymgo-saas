using GymGo.Domain.Users;

namespace GymGo.Application.Common.Interfaces;

/// <summary>
/// Genera tokens JWT para usuarios autenticados.
/// </summary>
public interface IJwtTokenGenerator
{
    string Generate(User user, out DateTime expiresAtUtc);
}
