using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.VoteInPoll;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/announcements/{id}/poll/vote")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class VoteInPollEndpoint(IHandler<VoteInPollRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="body">The chosen options.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The answer is recorded (it replaces the previous one).</response>
    /// <response code="400">No option, an unknown option, several options for a single-answer poll, or the same option twice.</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    /// <response code="404">No poll on a published announcement that the person can read.</response>
    /// <response code="409">The poll is closed.</response>
    [HttpPut]
    [EndpointSummary("Answers a poll")]
    [EndpointDescription("Only club members answer. The answer can be changed until the closing date (included); afterwards nothing can change.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Vote(string id, VoteInPollBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new VoteInPollRequest(id, body), ct)).ToNoContent();
}
