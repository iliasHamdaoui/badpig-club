using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.RemoveCaptain;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/admin/interclubs/teams/{id}/captain")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class RemoveCaptainEndpoint(IHandler<RemoveCaptainRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the team.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The team has no captain.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="404">No team with this identifier.</response>
    [HttpDelete]
    [EndpointSummary("Removes the captain of a team")]
    [EndpointDescription("The team has no captain any more (nobody can then change its fixtures, except the board).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCaptain(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new RemoveCaptainRequest(id), ct)).ToNoContent();
}
