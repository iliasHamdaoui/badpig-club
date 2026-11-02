using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.AddAnnouncementComment;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/announcements/{id}/comments")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class AddAnnouncementCommentEndpoint(IHandler<AddAnnouncementCommentRequest, CreatedResponse> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="body">The comment.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="201">The comment is added: its identifier is returned.</response>
    /// <response code="400">Empty or too long comment (see the errors).</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    /// <response code="404">No published announcement with this identifier that the person can read, or no such comment to answer on it.</response>
    /// <response code="409">Comments are closed on this announcement.</response>
    [HttpPost]
    [EndpointSummary("Comments an announcement")]
    [EndpointDescription("A club member (or the board) writes a comment, as plain text, under a published announcement or poll they can read. Give parentId to answer a comment (any earlier one): the answer shows a quote of it. The comment shows the name of its author. Refused (409) when the board closed the comments. The author of the announcement, and the author of the comment that is answered, receive an e-mail with the comment.")]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddComment(string id, AddAnnouncementCommentBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new AddAnnouncementCommentRequest(id, body), ct)).ToCreated();
}
