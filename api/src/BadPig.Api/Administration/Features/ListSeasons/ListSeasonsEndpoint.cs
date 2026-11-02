using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.ListSeasons;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/seasons")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListSeasonsEndpoint(IHandler<ListSeasonsRequest, PagedResponse<ListSeasonsItem>> handler) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Lists the seasons")]
    [EndpointDescription("With their dates, grace period and fees; the current season is flagged. Search on the name; sort on startsOn (default, most recent first) or name; paginated.")]
    [ProducesResponseType<PagedResponse<ListSeasonsItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromQuery] ListSeasonsRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
