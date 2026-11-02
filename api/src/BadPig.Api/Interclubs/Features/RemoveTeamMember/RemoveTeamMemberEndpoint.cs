using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.RemoveTeamMember;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/teams/{id}/members/{accountId}")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class RemoveTeamMemberEndpoint(IHandler<RemoveTeamMemberRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the team.</param>
    /// <param name="accountId">Account identifier of the person.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The person is no longer a player of the team.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="403">The person is neither the captain of this team nor on the board.</response>
    /// <response code="404">No team with this identifier, or this person is not a player of it.</response>
    [HttpDelete]
    [EndpointSummary("Removes a player from a team")]
    [EndpointDescription("The captain of the team (or the board) removes a player. The answers of this person to the proposals of dates still open for the team are deleted; those of closed proposals are kept.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTeamMember(string id, string accountId, CancellationToken ct) =>
        (await handler.HandleAsync(new RemoveTeamMemberRequest(id, accountId), ct)).ToNoContent();
}
