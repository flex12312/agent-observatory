namespace AgentObservatory.Contracts;

public sealed record TelemetryBatch
{
    public const int CurrentSchemaVersion = 1;

    public required int SchemaVersion { get; init; }
    public required Guid ProjectId { get; init; }
    public required IReadOnlyList<TelemetryEvent> Events { get; init; }
}
