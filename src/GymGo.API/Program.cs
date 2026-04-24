using GymGo.API.Endpoints;
using GymGo.API.Extensions;
using GymGo.API.Middleware;
using GymGo.Application;
using GymGo.Infrastructure;
using GymGo.Infrastructure.Persistence;
using Serilog;

// ──────────────────────────────────────────────────────────────────────
// 1. BOOTSTRAP LOGGER (antes que builder, por si falla la configuración)
// ──────────────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Iniciando GymGo.API...");

    var builder = WebApplication.CreateBuilder(args);

    // ──────────────────────────────────────────────────────────────────
    // 2. SERILOG (lee de appsettings.json -> "Serilog")
    // ──────────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, cfg) =>
    {
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext();
    });

    // ──────────────────────────────────────────────────────────────────
    // 3. SERVICIOS
    // ──────────────────────────────────────────────────────────────────
    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddJwtAuthentication(builder.Configuration)
        .AddGymGoCors(builder.Configuration)
        .AddSwaggerWithJwt();

    builder.Services.AddControllers();
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddHealthChecks()
        .AddSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")!,
            name: "sqlserver",
            tags: new[] { "db", "ready" },
            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded);

    var app = builder.Build();

    // ──────────────────────────────────────────────────────────────────
    // 4. PIPELINE
    // ──────────────────────────────────────────────────────────────────
    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(opts =>
        {
            opts.SwaggerEndpoint("/swagger/v1/swagger.json", "GymGo API v1");
            opts.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();
    app.UseCors(CorsExtensions.PolicyName);

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseMiddleware<TenantResolutionMiddleware>();

    app.MapHealthEndpoints();
    app.MapAuthEndpoints();
    app.MapMemberEndpoints();
    app.MapMembershipPlanEndpoints();
    app.MapMembershipAssignmentEndpoints();
    app.MapControllers();

    Log.Information("GymGo.API listo. Escuchando...");
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "GymGo.API falló al iniciar.");
}
finally
{
    Log.CloseAndFlush();
}

// Hace Program público para WebApplicationFactory<Program> en tests.
public partial class Program { }
