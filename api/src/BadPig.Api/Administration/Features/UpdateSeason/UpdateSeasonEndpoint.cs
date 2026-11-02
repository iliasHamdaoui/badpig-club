using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.UpdateSeason;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/seasons/{id}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class UpdateSeasonEndpoint(IHandler<UpdateSeasonRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the season (the <c>id</c> returned by the seasons list).</param>
    /// <param name="body">New values of the season.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The season is updated.</response>
    /// <response code="400">Invalid values (see the errors).</response>
    /// <response code="404">No season with this identifier.</response>
    /// <response code="409">The name or the dates collide with another season.</response>
    [HttpPut]
    [EndpointSummary("Updates a season")]
    [EndpointDescription("Changes the name, the dates, the grace period and the fees. The memberships already validated keep the amount they were validated with.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(string id, UpdateSeasonBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new UpdateSeasonRequest(id, body), ct)).ToNoContent();
}
