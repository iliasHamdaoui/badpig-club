using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.SetTitle;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/users/{id}/title")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class SetTitleEndpoint(IHandler<SetTitleRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the account (the <c>id</c> returned by the users list).</param>
    /// <param name="body">JSON body of the request.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The title is assigned (or removed).</response>
    /// <response code="404">No account or no title with these identifiers.</response>
    /// <response code="409">Only an active board member can have a title.</response>
    [HttpPut]
    [EndpointSummary("Assigns or removes a title")]
    [EndpointDescription("Only an active board member can have a title; the title moves to the new person if it had a holder. An empty titleId removes the title.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Set(string id, SetTitleBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new SetTitleRequest(id, body.TitleId), ct)).ToNoContent();
}
