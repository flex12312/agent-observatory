using AgentObservatory.Contracts;
using System.Text.Json;

namespace AgentObservatory.Api.Controllers;

internal static class TelemetryBatchValidator
{
    internal const int MaxEvents = 100;

    public static Dictionary<string, string[]> Validate(TelemetryBatch batch)
    {
        var errors = new Dictionary<string, string[]>();
        void Add(string path, string message) => errors[path] = [message];

        if (batch.SchemaVersion != TelemetryBatch.CurrentSchemaVersion)
            Add("schemaVersion", "Only schema version 1 is supported.");
        if (batch.ProjectId == Guid.Empty)
            Add("projectId", "A non-empty project ID is required.");
        if (batch.Events is null || batch.Events.Count == 0)
        {
            Add("events", "At least one event is required.");
            return errors;
        }
        if (batch.Events.Count > MaxEvents)
        {
            Add("events", $"A batch may contain at most {MaxEvents} events.");
            return errors;
        }

        var ids = new HashSet<Guid>();
        for (var i = 0; i < batch.Events.Count; i++)
        {
            var e = batch.Events[i];
            var prefix = $"events[{i}]";
            if (e is null) { Add(prefix, "Event cannot be null."); continue; }
            if (e.EventId == Guid.Empty) Add(prefix + ".eventId", "A non-empty event ID is required.");
            else if (!ids.Add(e.EventId)) Add(prefix + ".eventId", "Event ID is duplicated within the batch.");
            if (e.RunId == Guid.Empty) Add(prefix + ".runId", "A non-empty run ID is required.");
            if (e.OperationId == Guid.Empty) Add(prefix + ".operationId", "A non-empty operation ID is required.");
            if (e.ParentId == Guid.Empty) Add(prefix + ".parentId", "Parent ID must not be empty.");
            if (e.AgentId == Guid.Empty) Add(prefix + ".agentId", "Agent ID must not be empty.");
            if (e.Timestamp == default || e.Timestamp.Offset != TimeSpan.Zero)
                Add(prefix + ".timestamp", "A UTC timestamp is required.");

            var isRun = e.Type is TelemetryEventType.RunStarted or TelemetryEventType.RunFinished;
            var isStarted = e.Type is TelemetryEventType.RunStarted or TelemetryEventType.AgentStarted
                or TelemetryEventType.ModelCallStarted or TelemetryEventType.ToolCallStarted;
            var isFinished = e.Type is TelemetryEventType.RunFinished or TelemetryEventType.AgentFinished
                or TelemetryEventType.ModelCallFinished or TelemetryEventType.ToolCallFinished;
            if (!Enum.IsDefined(e.Type)) Add(prefix + ".type", "Unknown event type.");
            if (!Enum.IsDefined(e.Status)) Add(prefix + ".status", "Unknown status.");
            if ((isStarted || e.Type == TelemetryEventType.Retry) && e.Status != TelemetryStatus.Running)
                Add(prefix + ".status", "Started and retry events must be running.");
            if (isFinished && e.Status is not (TelemetryStatus.Succeeded or TelemetryStatus.Failed or TelemetryStatus.Cancelled))
                Add(prefix + ".status", "Finished events require a final status.");
            if (e.Type == TelemetryEventType.Error && e.Status != TelemetryStatus.Failed)
                Add(prefix + ".status", "Error events must have failed status.");
            if (isRun && (e.OperationId != e.RunId || e.ParentId is not null || e.AgentId is not null))
                Add(prefix, "Run events must use runId as operationId and have no parent or agent.");
            if (e.Type is TelemetryEventType.AgentStarted or TelemetryEventType.AgentFinished or
                TelemetryEventType.ModelCallStarted or TelemetryEventType.ModelCallFinished or
                TelemetryEventType.ToolCallStarted or TelemetryEventType.ToolCallFinished)
            {
                if (e.AgentId is null) Add(prefix + ".agentId", "An agent ID is required for this event.");
                if (e.ParentId is null) Add(prefix + ".parentId", "A parent operation ID is required for this event.");
            }
            if (e.DurationMs is not null && (!isFinished || !double.IsFinite(e.DurationMs.Value) || e.DurationMs < 0))
                Add(prefix + ".durationMs", "Duration must be finite, non-negative and used only on finished events.");
            if (e.Usage is not null && (e.Type != TelemetryEventType.ModelCallFinished ||
                string.IsNullOrWhiteSpace(e.Usage.Model) || e.Usage.InputTokens < 0 || e.Usage.OutputTokens < 0))
                Add(prefix + ".usage", "Usage requires a finished model call, a model name and non-negative token counts.");
            if (e.Type == TelemetryEventType.Retry && (e.Metadata is null ||
                !e.Metadata.TryGetValue("attempt", out var attempt) || attempt.ValueKind != JsonValueKind.Number ||
                !attempt.TryGetInt32(out var count) || count < 2))
                Add(prefix + ".metadata.attempt", "Retry attempt must be an integer of at least 2.");
        }
        return errors;
    }
}
