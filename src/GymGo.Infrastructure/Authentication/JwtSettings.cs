namespace GymGo.Infrastructure.Authentication;

/// <summary>
/// Bind a appsettings.json: "JwtSettings": { ... }.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string Secret { get; set; } = default!;
    public int ExpirationMinutes { get; set; } = 60;
}
