using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.DeleteAnnouncementComment;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/announcements/{id}/comments/{commentId}")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class DeleteAnnouncementCommentEndpoint(IHandler<DeleteAnnouncementCommentRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="commentId">Identifier of the comment.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The comment is deleted.</response>
    /// <response code="403">The person is not a club member, or the comment is not theirs and they are not a board member.</response>
    /// <response code="404">No such comment on a published announcement that the person can read.</response>
    [HttpDelete]
    [EndpointSummary("Deletes a comment")]
    [EndpointDescription("A member deletes their own comment; any board member deletes any comment.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, string commentId, CancellationToken ct) =>
        (await handler.HandleAsync(new DeleteAnnouncementCommentRequest(id, commentId), ct)).ToNoContent();
}
