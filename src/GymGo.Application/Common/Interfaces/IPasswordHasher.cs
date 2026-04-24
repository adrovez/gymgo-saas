namespace GymGo.Application.Common.Interfaces;

/// <summary>
/// Hash y verificación de contraseñas. Implementado en Infrastructure
/// con BCrypt.Net.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
