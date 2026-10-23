using System.Text.Json;
using System.Text.Json.Nodes;
using AgentObservatory.Contracts;
using Xunit;

namespace AgentObservatory.Api.Tests;

public sealed class EventContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Example_preserves_operation_links_and_retry_on_round_trip()
    {
        var batch = JsonSerializer.Deserialize<TelemetryBatch>(ReadExample(), JsonOptions)!;

        Assert.Equal(TelemetryBatch.CurrentSchemaVersion, batch.SchemaVersion);
        Assert.Equal(14, batch.Events.Count);
        Assert.Equal(batch.Events.Count, batch.Events.Select(e => e.EventId).Distinct().Count());

        var encoded = JsonSerializer.Serialize(batch, JsonOptions);
        var restored = JsonSerializer.Deserialize<TelemetryBatch>(encoded, JsonOptions)!;
        Assert.Equal(batch.Events.Select(e => e.EventId), restored.Events.Select(e => e.EventId));

        var retry = Assert.Single(restored.Events, e => e.Type == TelemetryEventType.Retry);
        var nextOperationId = retry.Metadata!["nextOperationId"].GetGuid();
        var nextStart = Assert.Single(restored.Events,
            e => e.Type == TelemetryEventType.ToolCallStarted && e.OperationId == nextOperationId);
        Assert.Equal(retry.AgentId, nextStart.AgentId);
        Assert.Equal(retry.ParentId, nextStart.ParentId);
        Assert.NotEqual(retry.OperationId, nextStart.OperationId);
        Assert.Equal(2, retry.Metadata["attempt"].GetInt32());

        var call = Assert.Single(restored.Events, e => e.Type == TelemetryEventType.ModelCallFinished);
        Assert.Equal(120L, call.Usage!.InputTokens);
        Assert.Equal(40L, call.Usage.OutputTokens);
        Assert.All(restored.Events, e => Assert.Equal(TimeSpan.Zero, e.Timestamp.Offset));

        using var json = JsonDocument.Parse(encoded);
        Assert.Equal("runStarted", json.RootElement.GetProperty("events")[0].GetProperty("type").GetString());
        Assert.Equal("running", json.RootElement.GetProperty("events")[0].GetProperty("status").GetString());
    }

    [Theory]
    [InlineData("type", "17")]
    [InlineData("type", "\"unknownType\"")]
    [InlineData("status", "0")]
    [InlineData("status", "\"unknownStatus\"")]
    public void Unknown_or_numeric_enum_values_are_rejected(string field, string value)
    {
        var example = JsonNode.Parse(ReadExample())!;
        example["events"]![0]![field] = JsonNode.Parse(value);

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<TelemetryBatch>(example.ToJsonString(), JsonOptions));
    }

    [Fact]
    public void Missing_event_identifier_is_rejected()
    {
        var example = JsonNode.Parse(ReadExample())!;
        example["events"]![0]!.AsObject().Remove("eventId");

        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<TelemetryBatch>(example.ToJsonString(), JsonOptions));
    }

    [Fact]
    public void Additional_fields_do_not_break_reading_existing_events()
    {
        var example = JsonNode.Parse(ReadExample())!;
        example["events"]![0]!["additionalField"] = "optional extension";

        var batch = JsonSerializer.Deserialize<TelemetryBatch>(example.ToJsonString(), JsonOptions)!;

        Assert.Equal(TelemetryEventType.RunStarted, batch.Events[0].Type);
    }

    private static string ReadExample() =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "run.json"));
}
