using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.DeleteSchoolHoliday;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/seasons/{seasonId}/holidays/{id}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class DeleteSchoolHolidayEndpoint(IHandler<DeleteSchoolHolidayRequest> handler) : ControllerBase
{
    /// <param name="seasonId">Identifier of the season.</param>
    /// <param name="id">Identifier of the period of holidays.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The period is deleted.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="404">No period of this season with this identifier.</response>
    [HttpDelete]
    [EndpointSummary("Deletes a period of school holidays")]
    [EndpointDescription("The sessions of those days come back on the site.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHoliday(string seasonId, string id, CancellationToken ct) =>
        (await handler.HandleAsync(new DeleteSchoolHolidayRequest(seasonId, id), ct)).ToNoContent();
}
