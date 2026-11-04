using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Usage.Features.RecordPageVisit;

[ApiController]
[Tags("Usage")]
[Route(ApiRoutes.V1 + "/usage/visits")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class RecordPageVisitEndpoint(IHandler<RecordPageVisitRequest> handler) : ControllerBase
{
    /// <param name="request">The page and the time it stayed visible.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The visit is added to the statistics of the day.</response>
    /// <response code="400">Invalid page name, number of views or duration (see the errors).</response>
    [HttpPost]
    [EndpointSummary("Reports the use of a page")]
    [EndpointDescription("Called by the website when a signed-in person leaves a page or hides the tab. The page is a route template (for example /announcements/:id), never an address with identifiers. Only a counter per day and per page is increased: the person is not recorded, nor the address of the device.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Record([FromBody] RecordPageVisitRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToNoContent();
}
