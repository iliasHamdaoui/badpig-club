using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.CreateSchoolHoliday;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/seasons/{seasonId}/holidays")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CreateSchoolHolidayEndpoint(IHandler<CreateSchoolHolidayRequest, CreatedResponse> handler) : ControllerBase
{
    /// <param name="seasonId">Identifier of the season.</param>
    /// <param name="body">The period of holidays.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="201">The period is created: its identifier is returned.</response>
    /// <response code="400">Invalid name, invalid dates, or dates outside the season (see the errors).</response>
    /// <response code="404">No season with this identifier.</response>
    /// <response code="409">These dates overlap another period of the season.</response>
    [HttpPost]
    [EndpointSummary("Adds school holidays to a season")]
    [EndpointDescription("During these days the club is closed: the site says there is no session and shows the next one. Both days are included; the period stays inside the season and does not overlap another one.")]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateHoliday(string seasonId, CreateSchoolHolidayBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new CreateSchoolHolidayRequest(seasonId, body), ct)).ToCreated();
}
