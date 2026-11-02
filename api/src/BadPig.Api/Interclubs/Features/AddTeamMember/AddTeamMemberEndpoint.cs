using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.AddTeamMember;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/teams/{id}/members/{accountId}")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class AddTeamMemberEndpoint(IHandler<AddTeamMemberRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the team.</param>
    /// <param name="accountId">Account identifier of the person.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The person is a player of the team.</response>
    /// <response code="400">Invalid identifier, or the person is not an active club member (see the errors).</response>
    /// <response code="403">The person is neither the captain of this team nor on the board.</response>
    /// <response code="404">No team with this identifier.</response>
    [HttpPut]
    [EndpointSummary("Adds a player to a team")]
    [EndpointDescription("The captain of the team (or the board) adds an active club member to the players of the team. Only the players of a team can answer its proposals of dates. Adding a player who is already in the team changes nothing.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddTeamMember(string id, string accountId, CancellationToken ct) =>
        (await handler.HandleAsync(new AddTeamMemberRequest(id, accountId), ct)).ToNoContent();
}
