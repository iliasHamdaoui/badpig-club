using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.SetMatchResult;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/fixtures/{id}/matches/{slot}/result")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class SetMatchResultEndpoint(IHandler<SetMatchResultRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the fixture.</param>
    /// <param name="slot">The match in the fixture: SD, MX1, MX2, DD, DH, SH1 or SH2.</param>
    /// <param name="body">How this match ended.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The result of the match is recorded.</response>
    /// <response code="400">Invalid identifier, unknown match, sets that do not make a finished match, or a forfeit or retirement without the side that gave up (see the errors).</response>
    /// <response code="403">The person is neither the captain of the team nor on the board.</response>
    /// <response code="404">No fixture with this identifier.</response>
    /// <response code="409">The fixture has not been played yet.</response>
    [HttpPut]
    [EndpointSummary("Enters the result of one match of a fixture")]
    [EndpointDescription("The captain of the team (or the board) enters one match at a time, as the matches finish, instead of the whole fixture at once. Same rules as the result of a fixture: played (2 or 3 sets), forfeit or retirement (the side that gave up loses). Nobody types who won. Sending this match again replaces it; the other matches are left untouched, and the score of the fixture stays unknown until the seven matches are decided. Only for a fixture of today or earlier (date in Paris).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetMatchResult(string id, string slot, SetMatchResultBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new SetMatchResultRequest(id, slot, body), ct)).ToNoContent();
}
