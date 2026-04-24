using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace GymGo.API.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapGet("/api/v1/ping", () => Results.Ok(new
        {
            status = "ok",
            service = "GymGo.API",
            utc = DateTime.UtcNow
        })).WithTags("System");

        return app;
    }
}
