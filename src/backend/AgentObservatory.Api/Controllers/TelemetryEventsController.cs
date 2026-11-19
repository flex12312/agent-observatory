using System.Data;
using System.Text.Json;
using AgentObservatory.Contracts;
using AgentObservatory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace AgentObservatory.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
public sealed class TelemetryEventsController(ObservatoryDbContext db, IWebHostEnvironment environment) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [HttpPost("events")]
    [RequestSizeLimit(1_048_576)]
    public async Task<IActionResult> Ingest([FromBody] TelemetryBatch batch, CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment()) return NotFound();
        var errors = TelemetryBatchValidator.Validate(batch);
        if (errors.Count > 0) return ValidationProblem(new ValidationProblemDetails(errors));

        await db.Database.OpenConnectionAsync(cancellationToken);
        var connection = (NpgsqlConnection)db.Database.GetDbConnection();
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        // Concurrent batches acquire row locks in the same order.
        foreach (var item in batch.Events.OrderBy(item => item.EventId))
        {
            await using var command = new NpgsqlCommand("""
                INSERT INTO telemetry_events (project_id, event_id, run_id, occurred_at, payload)
                VALUES (@project, @event, @run, @occurred, @payload)
                ON CONFLICT (project_id, event_id) DO UPDATE
                  SET event_id = EXCLUDED.event_id
                  WHERE telemetry_events.payload = EXCLUDED.payload
                RETURNING event_id
                """, connection, transaction);
            command.Parameters.AddWithValue("project", batch.ProjectId);
            command.Parameters.AddWithValue("event", item.EventId);
            command.Parameters.AddWithValue("run", item.RunId);
            command.Parameters.AddWithValue("occurred", item.Timestamp);
            command.Parameters.AddWithValue("payload", NpgsqlDbType.Jsonb, JsonSerializer.Serialize(item, JsonOptions));
            if (await command.ExecuteScalarAsync(cancellationToken) is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Conflict(new ProblemDetails { Title = "Event ID already exists with different content", Status = 409 });
            }
        }
        await transaction.CommitAsync(cancellationToken);
        return Ok(new { accepted = batch.Events.Count });
    }

    // Bounded first page for the local demo; pagination and authorization are separate work.
    [HttpGet("projects/{projectId:guid}/runs/{runId:guid}/events")]
    public async Task<IActionResult> GetEvents(Guid projectId, Guid runId, CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment()) return NotFound();
        if (projectId == Guid.Empty || runId == Guid.Empty) return BadRequest();
        await db.Database.OpenConnectionAsync(cancellationToken);
        var connection = (NpgsqlConnection)db.Database.GetDbConnection();
        await using var command = new NpgsqlCommand("""
            SELECT payload FROM telemetry_events
            WHERE project_id = @project AND run_id = @run
            ORDER BY occurred_at, event_id LIMIT 101
            """, connection);
        command.Parameters.AddWithValue("project", projectId);
        command.Parameters.AddWithValue("run", runId);
        var events = new List<JsonElement>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            using var document = JsonDocument.Parse(reader.GetString(0));
            events.Add(document.RootElement.Clone());
        }
        var hasMore = events.Count > 100;
        if (hasMore) events.RemoveAt(100);
        return Ok(new { events, hasMore });
    }
}
