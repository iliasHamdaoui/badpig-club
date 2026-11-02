using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.DeleteSeason;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/seasons/{id}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class DeleteSeasonEndpoint(IHandler<DeleteSeasonRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the season (the <c>id</c> returned by the seasons list).</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The season is deleted.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="404">No season with this identifier.</response>
    /// <response code="409">The season has validated memberships or interclub teams.</response>
    [HttpDelete]
    [EndpointSummary("Deletes a season")]
    [EndpointDescription("Only a season without any validated membership and without any interclub team can be deleted.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new DeleteSeasonRequest(id), ct)).ToNoContent();
}
