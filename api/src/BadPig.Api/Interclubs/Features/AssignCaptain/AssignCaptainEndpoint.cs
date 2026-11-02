using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.AssignCaptain;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/admin/interclubs/teams/{id}/captain")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class AssignCaptainEndpoint(IHandler<AssignCaptainRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the team.</param>
    /// <param name="body">The account of the captain.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The captain is designated.</response>
    /// <response code="400">Invalid identifier, or the person is not an active club member (see the errors).</response>
    /// <response code="404">No team with this identifier.</response>
    [HttpPut]
    [EndpointSummary("Designates the captain of a team")]
    [EndpointDescription("Designates a club member as the captain of the team (replacing the previous one). The captain does not have to be on the board: it is the only right the person gets, on this team.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignCaptain(string id, AssignCaptainBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new AssignCaptainRequest(id, body), ct)).ToNoContent();
}
