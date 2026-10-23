using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Xunit;

namespace AgentObservatory.Api.Tests;

public sealed class ApiAvailabilityTests
{
    [Fact]
    public async Task Api_stays_available_when_database_cannot_be_reached()
    {
        // Reserve an unused port without listening so no real database can be contacted.
        using var reservedPort = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        reservedPort.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        var port = ((IPEndPoint)reservedPort.LocalEndPoint!).Port;

        using var factory = new BackendFactory(
            $"Host=127.0.0.1;Port={port};Database=unavailable;Username=test;Password=test;Timeout=1;Pooling=false");
        using var client = factory.CreateClient();

        using var readiness = await client.GetAsync("/health/ready");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, readiness.StatusCode);
        Assert.Equal("Unhealthy", await readiness.Content.ReadAsStringAsync());

        using var liveness = await client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, liveness.StatusCode);

        using var openApi = await client.GetAsync("/openapi/v1.json");
        Assert.Equal(HttpStatusCode.OK, openApi.StatusCode);
        using var document = JsonDocument.Parse(await openApi.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.TryGetProperty("openapi", out _));

        using var missing = await client.GetAsync("/missing");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Equal("application/problem+json", missing.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(await missing.Content.ReadAsStringAsync());
        Assert.Equal(404, problem.RootElement.GetProperty("status").GetInt32());
    }
}
