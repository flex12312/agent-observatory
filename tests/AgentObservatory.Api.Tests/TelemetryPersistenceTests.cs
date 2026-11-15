using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace AgentObservatory.Api.Tests;

public sealed class TelemetryPersistenceTests
{
    [Fact]
    [Trait("Category", "Database")]
    public async Task Events_are_persisted_retried_and_conflicting_batches_rolled_back()
    {
        await using var database = new PostgreSqlBuilder("postgres:17-alpine").Build();
        await database.StartAsync();
        await using (var connection = new NpgsqlConnection(database.GetConnectionString()))
        {
            await connection.OpenAsync();
            var sql = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "001_telemetry_events.sql"));
            await using var migration = new NpgsqlCommand(sql, connection);
            await migration.ExecuteNonQueryAsync();
        }
        using var factory = new BackendFactory(database.GetConnectionString());
        using var client = factory.CreateClient();
        var batch = JsonNode.Parse(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "run.json")))!;
        var projectId = (string)batch["projectId"]!;
        var runId = (string)batch["events"]![0]!["runId"]!;
        var endpoint = $"/api/telemetry/projects/{projectId}/runs/{runId}/events";

        var requests = await Task.WhenAll(
            client.PostAsJsonAsync("/api/telemetry/events", batch),
            client.PostAsJsonAsync("/api/telemetry/events", batch));
        Assert.All(requests, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        foreach (var response in requests) response.Dispose();
        var result = await client.GetFromJsonAsync<JsonNode>(endpoint);
        Assert.Equal(14, result!["events"]!.AsArray().Count);
        Assert.False((bool)result["hasMore"]!);

        // The first event is new; the second reuses an ID with different content.
        var conflicting = batch.DeepClone();
        conflicting["events"]![0]!["eventId"] = Guid.NewGuid().ToString();
        conflicting["events"]![1]!["name"] = "changed";
        using var conflict = await client.PostAsJsonAsync("/api/telemetry/events", conflicting);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        result = await client.GetFromJsonAsync<JsonNode>(endpoint);
        Assert.Equal(14, result!["events"]!.AsArray().Count);

        var otherProject = await client.GetFromJsonAsync<JsonNode>(
            $"/api/telemetry/projects/{Guid.NewGuid()}/runs/{runId}/events");
        Assert.Empty(otherProject!["events"]!.AsArray());

        using var restartedFactory = new BackendFactory(database.GetConnectionString());
        using var restartedClient = restartedFactory.CreateClient();
        var afterRestart = await restartedClient.GetFromJsonAsync<JsonNode>(endpoint);
        Assert.Equal(14, afterRestart!["events"]!.AsArray().Count);
    }

    [Fact]
    public async Task Storage_endpoints_are_unavailable_in_production()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("ConnectionStrings:Database", "Host=127.0.0.1;Port=1;Database=test;Username=test;Password=test");
        });
        using var client = factory.CreateClient();
        var batch = JsonNode.Parse(await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures", "run.json")))!;
        using var post = await client.PostAsJsonAsync("/api/telemetry/events", batch);
        Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
        using var get = await client.GetAsync($"/api/telemetry/projects/{Guid.NewGuid()}/runs/{Guid.NewGuid()}/events");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }
}
