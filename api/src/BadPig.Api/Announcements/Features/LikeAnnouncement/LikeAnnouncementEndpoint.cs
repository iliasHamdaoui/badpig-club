using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.LikeAnnouncement;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/announcements/{id}/like")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class LikeAnnouncementEndpoint(IHandler<LikeAnnouncementRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The announcement is liked by the signed-in member.</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    /// <response code="404">No published announcement with this identifier that the person can read.</response>
    [HttpPost]
    [EndpointSummary("Likes an announcement")]
    [EndpointDescription("A club member likes a published announcement they can read. Nothing changes if they already liked it.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LikeAnnouncement(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new LikeAnnouncementRequest(id), ct)).ToNoContent();
}
