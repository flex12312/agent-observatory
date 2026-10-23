using System.Net;
using Testcontainers.PostgreSql;
using Xunit;

namespace AgentObservatory.Api.Tests;

public sealed class DatabaseAvailabilityTests
{
    [Fact]
    [Trait("Category", "Database")]
    public async Task Readiness_tracks_database_outage_and_recovery()
    {
        await using var database = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await database.StartAsync();

        using var factory = new BackendFactory(database.GetConnectionString() + ";Timeout=2;Command Timeout=2");
        using var client = factory.CreateClient();

        await AssertReadinessAsync(client, HttpStatusCode.OK);

        await database.StopAsync();
        await AssertReadinessAsync(client, HttpStatusCode.ServiceUnavailable);
        using var liveness = await client.GetAsync("/health/live");
        Assert.Equal(HttpStatusCode.OK, liveness.StatusCode);

        await database.StartAsync();
        await AssertReadinessAsync(client, HttpStatusCode.OK);
    }

    private static async Task AssertReadinessAsync(HttpClient client, HttpStatusCode expected)
    {
        HttpStatusCode? actual = null;
        for (var attempt = 0; attempt < 20; attempt++)
        {
            using var response = await client.GetAsync("/health/ready");
            actual = response.StatusCode;
            if (actual == expected)
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500));
        }

        Assert.Equal(expected, actual);
    }
}
