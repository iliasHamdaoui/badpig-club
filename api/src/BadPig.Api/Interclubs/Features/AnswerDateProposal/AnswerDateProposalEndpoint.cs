using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.AnswerDateProposal;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/proposals/{id}/answers")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class AnswerDateProposalEndpoint(IHandler<AnswerDateProposalRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the proposal.</param>
    /// <param name="body">The answers.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The answers are recorded.</response>
    /// <response code="400">Invalid identifier, unknown date or answer other than yes, maybe, no (see the errors).</response>
    /// <response code="403">The person is not a club member with an up-to-date membership, or is not a player of the team of the proposal.</response>
    /// <response code="404">No proposal with this identifier.</response>
    /// <response code="409">A date was already chosen: the proposal is closed.</response>
    [HttpPut]
    [EndpointSummary("Answers a proposal of dates")]
    [EndpointDescription("The signed-in person says, for one or several dates of the proposal, whether they are available (yes), maybe or not (no). A new answer replaces the previous one. Reserved to the players of the team of the proposal (the captain adds them to the team).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AnswerDateProposal(string id, AnswerDateProposalBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new AnswerDateProposalRequest(id, body), ct)).ToNoContent();
}
