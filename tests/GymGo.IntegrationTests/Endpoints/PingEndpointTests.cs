using System.Net;
using System.Text.Json;
using FluentAssertions;
using GymGo.IntegrationTests.Infrastructure;

namespace GymGo.IntegrationTests.Endpoints;

public class PingEndpointTests : IClassFixture<GymGoWebApplicationFactory>
{
    private readonly GymGoWebApplicationFactory _factory;

    public PingEndpointTests(GymGoWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Ping_devuelve_200_y_status_ok()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/ping");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("status").GetString().Should().Be("ok");
        doc.RootElement.GetProperty("service").GetString().Should().Be("GymGo.API");
    }
}
