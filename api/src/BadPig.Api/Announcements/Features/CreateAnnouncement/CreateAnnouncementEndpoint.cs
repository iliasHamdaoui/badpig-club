using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.CreateAnnouncement;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/admin/announcements")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CreateAnnouncementEndpoint(IHandler<CreateAnnouncementRequest, CreatedResponse> handler) : ControllerBase
{
    /// <response code="201">The announcement is created as a draft: its identifier is returned.</response>
    /// <response code="400">Invalid title, content, type or end date (see the errors).</response>
    [HttpPost]
    [EndpointSummary("Creates an announcement")]
    [EndpointDescription("The announcement starts as a draft (not published). The author is the signed-in board member and is only shown to the board. Add the poster and the attachments afterwards, then publish.")]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateAnnouncementRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToCreated();
}
