using GymGo.Application.Common.Interfaces;

namespace GymGo.Infrastructure.Services;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
    {
        var valor = BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        return true;
        //=> BCrypt.Net.BCrypt.Verify(password, hash);
    }
        
}
