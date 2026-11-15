using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AgentObservatory.Api.Tests;

public sealed class TelemetryValidationTests
{
    private static JsonNode Example() => JsonNode.Parse(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "run.json")))!;

    [Fact]
    public async Task Valid_example_can_be_checked_without_database()
    {
        using var factory = new BackendFactory("Host=127.0.0.1;Port=1;Database=test;Username=test;Password=test;Timeout=1");
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/telemetry/validate", Example());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = JsonNode.Parse(await response.Content.ReadAsStringAsync())!;
        Assert.True((bool)result["valid"]!);
        Assert.Equal(14, (int)result["eventCount"]!);
    }

    [Fact]
    public async Task Duplicate_ids_invalid_status_and_non_utc_timestamp_are_reported()
    {
        using var factory = new BackendFactory("Host=127.0.0.1;Port=1;Database=test;Username=test;Password=test;Timeout=1");
        using var client = factory.CreateClient();
        var example = Example();
        example["events"]![1]!["eventId"] = example["events"]![0]!["eventId"]!.DeepClone();
        example["events"]![0]!["status"] = "failed";
        example["events"]![0]!["timestamp"] = "2026-01-01T12:00:00+03:00";

        using var response = await client.PostAsJsonAsync("/api/telemetry/validate", example);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = JsonNode.Parse(await response.Content.ReadAsStringAsync())!["errors"]!;
        Assert.NotNull(errors["events[1].eventId"]);
        Assert.NotNull(errors["events[0].status"]);
        Assert.NotNull(errors["events[0].timestamp"]);
    }

    [Fact]
    public async Task Validation_is_unavailable_in_production()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("ConnectionStrings:Database", "Host=127.0.0.1;Port=1;Database=test;Username=test;Password=test");
        });
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/telemetry/validate", Example());
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Null_and_empty_batches_are_rejected()
    {
        using var factory = new BackendFactory("Host=127.0.0.1;Port=1;Database=test;Username=test;Password=test;Timeout=1");
        using var client = factory.CreateClient();
        var example = Example();
        example["events"] = new JsonArray();
        using var empty = await client.PostAsJsonAsync("/api/telemetry/validate", example);
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        example["events"] = null;
        using var nullEvents = await client.PostAsJsonAsync("/api/telemetry/validate", example);
        Assert.Equal(HttpStatusCode.BadRequest, nullEvents.StatusCode);
    }
}
