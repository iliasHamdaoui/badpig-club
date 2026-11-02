using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.WithdrawPollVote;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/announcements/{id}/poll/vote")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class WithdrawPollVoteEndpoint(IHandler<WithdrawPollVoteRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The answer is removed (nothing happens if there was none).</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    /// <response code="404">No poll on a published announcement that the person can read.</response>
    /// <response code="409">The poll is closed.</response>
    [HttpDelete]
    [EndpointSummary("Withdraws my answer to a poll")]
    [EndpointDescription("Possible until the closing date (included).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Withdraw(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new WithdrawPollVoteRequest(id), ct)).ToNoContent();
}
