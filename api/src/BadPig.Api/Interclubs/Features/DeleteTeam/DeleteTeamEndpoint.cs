using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.DeleteTeam;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/admin/interclubs/teams/{id}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class DeleteTeamEndpoint(IHandler<DeleteTeamRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the team.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The team and its fixtures are deleted.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="404">No team with this identifier.</response>
    [HttpDelete]
    [EndpointSummary("Deletes a team")]
    [EndpointDescription("Deletes a team and all its fixtures.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTeam(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new DeleteTeamRequest(id), ct)).ToNoContent();
}
