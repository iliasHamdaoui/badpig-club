using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.ChangePollDeadline;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/admin/announcements/{id}/poll/deadline")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ChangePollDeadlineEndpoint(IHandler<ChangePollDeadlineRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="body">The new closing date.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The closing date is changed.</response>
    /// <response code="400">Missing closing date, or a date in the past.</response>
    /// <response code="404">No poll on this announcement.</response>
    /// <response code="409">The poll is already closed.</response>
    [HttpPut]
    [EndpointSummary("Changes the closing date of a poll")]
    [EndpointDescription("Works even when members have already answered, as long as the poll is open. The announcement stays displayed at least until the poll closes.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeDeadline(string id, ChangePollDeadlineBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new ChangePollDeadlineRequest(id, body), ct)).ToNoContent();
}
