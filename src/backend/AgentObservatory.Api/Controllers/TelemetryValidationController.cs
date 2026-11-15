using AgentObservatory.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AgentObservatory.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
public sealed class TelemetryValidationController(IWebHostEnvironment environment) : ControllerBase
{
    // This endpoint only validates a batch. It does not ingest or store events.
    [HttpPost("validate")]
    [RequestSizeLimit(1_048_576)]
    public IActionResult ValidateBatch([FromBody] TelemetryBatch batch)
    {
        if (!environment.IsDevelopment())
            return NotFound();

        var errors = TelemetryBatchValidator.Validate(batch);
        if (errors.Count > 0)
            return ValidationProblem(new ValidationProblemDetails(errors));

        return Ok(new { valid = true, eventCount = batch.Events.Count });
    }
}

