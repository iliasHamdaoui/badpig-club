using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.RemoveAnnouncementAttachment;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/admin/announcements/{id}/attachments/{fileId}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class RemoveAnnouncementAttachmentEndpoint(IHandler<RemoveAnnouncementAttachmentRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="fileId">Identifier of the attachment (returned when it was added, and listed in the announcement).</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The attachment is removed.</response>
    /// <response code="404">No such attachment on this announcement.</response>
    [HttpDelete]
    [EndpointSummary("Removes an attachment of an announcement")]
    [EndpointDescription("The file is deleted from the server.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAttachment(string id, string fileId, CancellationToken ct) =>
        (await handler.HandleAsync(new RemoveAnnouncementAttachmentRequest(id, fileId), ct)).ToNoContent();
}
