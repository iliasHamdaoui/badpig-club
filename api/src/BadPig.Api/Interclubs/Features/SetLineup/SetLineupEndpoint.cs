using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.SetLineup;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/fixtures/{id}/lineup")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class SetLineupEndpoint(IHandler<SetLineupRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the fixture.</param>
    /// <param name="body">The players of each match.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The lineup is recorded.</response>
    /// <response code="400">Invalid identifier, unknown or repeated slot, wrong number of players for a slot, or a player who is not in the team (see the errors).</response>
    /// <response code="403">The person is neither the captain of the team nor on the board.</response>
    /// <response code="404">No fixture with this identifier.</response>
    [HttpPut]
    [EndpointSummary("Sets the lineup of a fixture")]
    [EndpointDescription("The captain of the team (or the board) chooses the players of the matches: one player for a singles match (SD, SH1, SH2), two for a doubles or mixed match (MX1, MX2, DD, DH). All the players must be in the team. The slots sent replace the previous lineup; a slot that is left out becomes empty. The sets already entered are kept.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetLineup(string id, SetLineupBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new SetLineupRequest(id, body), ct)).ToNoContent();
}
