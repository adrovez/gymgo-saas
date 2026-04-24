namespace GymGo.API.Extensions;

public static class CorsExtensions
{
    public const string PolicyName = "GymGoCors";

    public static IServiceCollection AddGymGoCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowed = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? Array.Empty<string>();

        services.AddCors(opts =>
        {
            opts.AddPolicy(PolicyName, policy =>
            {
                if (allowed.Length == 0)
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                }
                else
                {
                    policy.WithOrigins(allowed)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                }
            });
        });

        return services;
    }
}
