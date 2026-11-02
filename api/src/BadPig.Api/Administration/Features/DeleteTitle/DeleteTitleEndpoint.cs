using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.DeleteTitle;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/titles/{id}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class DeleteTitleEndpoint(IHandler<DeleteTitleRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the title (the <c>id</c> returned by the titles list).</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The title is deleted; its holder loses it.</response>
    /// <response code="404">No title with this identifier.</response>
    [HttpDelete]
    [EndpointSummary("Deletes a title")]
    [EndpointDescription("The person who held it loses it.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTitle(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new DeleteTitleRequest(id), ct)).ToNoContent();
}
