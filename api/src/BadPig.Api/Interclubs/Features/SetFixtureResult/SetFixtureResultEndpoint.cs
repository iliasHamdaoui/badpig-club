using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.SetFixtureResult;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/fixtures/{id}/result")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class SetFixtureResultEndpoint(IHandler<SetFixtureResultRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the fixture.</param>
    /// <param name="body">The sets of the seven matches.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The result is recorded.</response>
    /// <response code="400">Invalid identifier, a match missing or repeated, sets that do not make a finished match, or a forfeit or retirement without the side that gave up (see the errors).</response>
    /// <response code="403">The person is neither the captain of the team nor on the board.</response>
    /// <response code="404">No fixture with this identifier.</response>
    /// <response code="409">The fixture has not been played yet.</response>
    [HttpPut]
    [EndpointSummary("Enters the result of a fixture")]
    [EndpointDescription("The captain of the team (or the board) enters how each of the seven matches ended. Played: 2 or 3 sets (a set is won at 15 points with 2 points of lead; from 14-14 play goes on until a lead of 2, or until 21), won with 2 sets. Forfeit (not played) or retirement (stopped during the play): the side that gave up loses, and the sets completed so far may be given for a retirement. Nobody types who won: the outcome of each match is computed, and the score of the fixture from the matches won by each side. Sending the result again replaces the previous one. Only for a fixture of today or earlier (date in Paris).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetFixtureResult(string id, SetFixtureResultBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new SetFixtureResultRequest(id, body), ct)).ToNoContent();
}
