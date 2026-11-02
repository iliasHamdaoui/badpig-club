using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.CreatePollAnnouncement;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/admin/announcements/polls")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CreatePollAnnouncementEndpoint(IHandler<CreatePollAnnouncementRequest, CreatedResponse> handler) : ControllerBase
{
    /// <response code="201">The poll is created as a draft: its identifier is returned.</response>
    /// <response code="400">Invalid question, choices, closing date or end date (see the errors).</response>
    [HttpPost]
    [EndpointSummary("Creates a poll")]
    [EndpointDescription("A poll is an announcement of its own (form \"poll\"): the title is the question, the text an optional explanation. It starts as a draft. Only club members answer, until the closing date (included); the results are shown from the next day. Publish it like any announcement.")]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreatePollAnnouncementRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToCreated();
}
