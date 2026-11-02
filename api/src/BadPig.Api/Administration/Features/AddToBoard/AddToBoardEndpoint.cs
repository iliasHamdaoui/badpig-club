using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.AddToBoard;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/users/{id}/board/add")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class AddToBoardEndpoint(IHandler<AddToBoardRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the account (the <c>id</c> returned by the users list).</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The person is now on the board.</response>
    /// <response code="403">The signed-in person cannot change the board for this account (for example only an Admin can add an administrator).</response>
    /// <response code="404">No account with this identifier.</response>
    /// <response code="500">Board management is not configured on the server.</response>
    [HttpPost]
    [EndpointSummary("Adds a person to the board")]
    [EndpointDescription("The person joins the Board group. Only an Admin can add an administrator.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddToBoard(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new AddToBoardRequest(id), ct)).ToNoContent();
}
