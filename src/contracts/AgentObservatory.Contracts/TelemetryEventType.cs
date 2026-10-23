using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentObservatory.Contracts;

[JsonConverter(typeof(TelemetryEventTypeConverter))]
public enum TelemetryEventType
{
    RunStarted,
    RunFinished,
    AgentStarted,
    AgentFinished,
    ModelCallStarted,
    ModelCallFinished,
    ToolCallStarted,
    ToolCallFinished,
    Error,
    Retry
}

public sealed class TelemetryEventTypeConverter()
    : JsonStringEnumConverter<TelemetryEventType>(JsonNamingPolicy.CamelCase, allowIntegerValues: false);
