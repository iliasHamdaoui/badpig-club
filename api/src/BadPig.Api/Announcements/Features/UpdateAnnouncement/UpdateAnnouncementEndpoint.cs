using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.UpdateAnnouncement;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/admin/announcements/{id}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class UpdateAnnouncementEndpoint(IHandler<UpdateAnnouncementRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="body">New values of the announcement.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The announcement is updated.</response>
    /// <response code="400">Invalid title, content, type or end date (see the errors).</response>
    /// <response code="404">No announcement with this identifier.</response>
    [HttpPut]
    [EndpointSummary("Updates an announcement")]
    [EndpointDescription("Any board member can edit any announcement. The text is cleaned again. The announcement stays published or draft, pinned or not, and keeps its date, its poster and its attachments.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, UpdateAnnouncementBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new UpdateAnnouncementRequest(id, body), ct)).ToNoContent();
}
