using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentObservatory.Contracts;

[JsonConverter(typeof(TelemetryStatusConverter))]
public enum TelemetryStatus
{
    Running,
    Succeeded,
    Failed,
    Cancelled
}

public sealed class TelemetryStatusConverter()
    : JsonStringEnumConverter<TelemetryStatus>(JsonNamingPolicy.CamelCase, allowIntegerValues: false);
