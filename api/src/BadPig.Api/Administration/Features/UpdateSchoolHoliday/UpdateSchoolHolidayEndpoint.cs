using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.UpdateSchoolHoliday;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/seasons/{seasonId}/holidays/{id}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class UpdateSchoolHolidayEndpoint(IHandler<UpdateSchoolHolidayRequest> handler) : ControllerBase
{
    /// <param name="seasonId">Identifier of the season.</param>
    /// <param name="id">Identifier of the period of holidays.</param>
    /// <param name="body">The new values.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The period is changed.</response>
    /// <response code="400">Invalid name, invalid dates, or dates outside the season (see the errors).</response>
    /// <response code="404">No period of this season with this identifier.</response>
    /// <response code="409">These dates overlap another period of the season.</response>
    [HttpPut]
    [EndpointSummary("Changes a period of school holidays")]
    [EndpointDescription("Changes the name and the dates of a period. The period stays inside its season and does not overlap another one.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateHoliday(string seasonId, string id, UpdateSchoolHolidayBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new UpdateSchoolHolidayRequest(seasonId, id, body), ct)).ToNoContent();
}
