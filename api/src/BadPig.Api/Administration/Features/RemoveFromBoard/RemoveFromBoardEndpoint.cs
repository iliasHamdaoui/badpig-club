using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.RemoveFromBoard;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/users/{id}/board/remove")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class RemoveFromBoardEndpoint(IHandler<RemoveFromBoardRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the account (the <c>id</c> returned by the users list).</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The person left the board (they stay a member and lose their title).</response>
    /// <response code="403">The signed-in person cannot change the board for this account.</response>
    /// <response code="404">No account with this identifier.</response>
    /// <response code="409">At least one board member must remain.</response>
    /// <response code="500">Board management is not configured on the server.</response>
    [HttpPost]
    [EndpointSummary("Removes a person from the board")]
    [EndpointDescription("The person stays a member and loses their title. At least one board member must remain.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveFromBoard(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new RemoveFromBoardRequest(id), ct)).ToNoContent();
}
