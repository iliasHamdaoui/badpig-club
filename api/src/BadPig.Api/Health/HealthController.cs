using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Api.Health;

/// <summary>Public: used for monitoring (Docker healthcheck, alerts).</summary>
[ApiController]
[Route(ApiRoutes.V1 + "/health")]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    /// <response code="200">The API is running.</response>
    [HttpGet]
    [Tags("Health")]
    [EndpointSummary("Health check")]
    [EndpointDescription("Anonymous. Used by the container health checks.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get() => Ok(new { status = "ok" });
}
