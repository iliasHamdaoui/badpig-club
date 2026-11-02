using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.ListAnnouncements;

// No 401 here: visitors who are not signed in can read the public announcements.
[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/announcements")]
[AllowAnonymous]
public sealed class ListAnnouncementsEndpoint(IHandler<ListAnnouncementsRequest, PagedResponse<ListAnnouncementsItem>> handler) : ControllerBase
{
    /// <response code="200">One page of announcements, pinned first, then from the most recent publication.</response>
    /// <response code="400">Invalid sort field, type or status.</response>
    [HttpGet]
    [EndpointSummary("Lists the announcements")]
    [EndpointDescription("What is listed depends on the reader: any visitor sees the published public announcements; a club member with an up-to-date membership also sees the internal ones; the board sees everything, drafts and authors included. Search on title and excerpt; sort on publishedAt (default, most recent first), createdAt or title; filters kind and status; paginated.")]
    [ProducesResponseType<PagedResponse<ListAnnouncementsItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromQuery] ListAnnouncementsRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
