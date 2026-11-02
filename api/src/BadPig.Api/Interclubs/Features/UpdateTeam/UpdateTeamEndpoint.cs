using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.UpdateTeam;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/admin/interclubs/teams/{id}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class UpdateTeamEndpoint(IHandler<UpdateTeamRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the team.</param>
    /// <param name="body">The new values.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The team is changed.</response>
    /// <response code="400">Invalid name or season identifier (see the errors).</response>
    /// <response code="404">No team or no season with this identifier.</response>
    /// <response code="409">Another team already has this name for this season.</response>
    [HttpPut]
    [EndpointSummary("Changes a team")]
    [EndpointDescription("Changes the name and the season of a team. The captain and the fixtures are kept.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateTeam(string id, UpdateTeamBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new UpdateTeamRequest(id, body), ct)).ToNoContent();
}
