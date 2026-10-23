using System.Text.Json;

namespace AgentObservatory.Contracts;

public sealed record TelemetryEvent
{
    public required Guid EventId { get; init; }
    public required Guid RunId { get; init; }
    public required Guid OperationId { get; init; }
    public Guid? ParentId { get; init; }
    public Guid? AgentId { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required TelemetryEventType Type { get; init; }
    public required TelemetryStatus Status { get; init; }
    public double? DurationMs { get; init; }
    public string? Name { get; init; }
    public ModelUsage? Usage { get; init; }
    public IReadOnlyDictionary<string, JsonElement>? Metadata { get; init; }
}

public sealed record ModelUsage
{
    public required string Model { get; init; }
    public long? InputTokens { get; init; }
    public long? OutputTokens { get; init; }
}
