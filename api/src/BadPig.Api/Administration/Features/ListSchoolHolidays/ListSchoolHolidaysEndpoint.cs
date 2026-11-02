using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.ListSchoolHolidays;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/seasons/{seasonId}/holidays")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListSchoolHolidaysEndpoint(IHandler<ListSchoolHolidaysRequest, PagedResponse<ListSchoolHolidaysItem>> handler) : ControllerBase
{
    /// <param name="seasonId">Identifier of the season.</param>
    /// <param name="request">Search, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of holidays, the earliest first.</response>
    /// <response code="400">Unknown sort, invalid identifier or invalid page (see the errors).</response>
    /// <response code="404">No season with this identifier.</response>
    [HttpGet]
    [EndpointSummary("Lists the school holidays of a season")]
    [EndpointDescription("The periods during which the club closes (no session). Search on the name; sort on startsOn (default) or name; paginated.")]
    [ProducesResponseType<PagedResponse<ListSchoolHolidaysItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListHolidays(string seasonId, [FromQuery] ListSchoolHolidaysRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request with { SeasonId = seasonId }, ct)).ToOk();
}
