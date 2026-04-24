using GymGo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GymGo.IntegrationTests.Infrastructure;

/// <summary>
/// WebApplicationFactory<Program> que reemplaza el ApplicationDbContext
/// SQL Server por uno InMemory para que los tests no necesiten BD real.
/// </summary>
public class GymGoWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"GymGoTest_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Quita el DbContext registrado por AddInfrastructure
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbContextDescriptor is not null)
                services.Remove(dbContextDescriptor);

            // Re-registra con InMemory (cada test usa su propia base)
            services.AddDbContext<ApplicationDbContext>(opts =>
            {
                opts.UseInMemoryDatabase(_dbName);
            });

            // Asegura que el schema exista
            using var scope = services.BuildServiceProvider().CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            ctx.Database.EnsureCreated();
        });
    }
}
