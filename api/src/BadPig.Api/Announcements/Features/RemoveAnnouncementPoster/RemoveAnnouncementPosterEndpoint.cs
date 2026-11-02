using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.RemoveAnnouncementPoster;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/admin/announcements/{id}/poster")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class RemoveAnnouncementPosterEndpoint(IHandler<RemoveAnnouncementPosterRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The announcement has no poster any more.</response>
    /// <response code="404">No announcement with this identifier.</response>
    [HttpDelete]
    [EndpointSummary("Removes the poster of an announcement")]
    [EndpointDescription("Nothing happens if the announcement has no poster.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAnnouncementPoster(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new RemoveAnnouncementPosterRequest(id), ct)).ToNoContent();
}
