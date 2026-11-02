using BadPig.Administration.Core;
using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.ListTitles;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/titles")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListTitlesEndpoint(IHandler<ListTitlesRequest, PagedResponse<ListTitlesItem>> handler) : ControllerBase
{
    /// <response code="200">One page of titles with their holder.</response>
    /// <response code="400">Invalid sort field.</response>
    [HttpGet]
    [EndpointSummary("Lists the titles")]
    [EndpointDescription("With their holder. Search on the title or the holder's name; sort on label or createdAt (default: oldest first); paginated.")]
    [ProducesResponseType<PagedResponse<ListTitlesItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromQuery] ListTitlesRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
