using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.DeleteAccount;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/users/{id}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class DeleteAccountEndpoint(IHandler<DeleteAccountRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the account (the <c>id</c> returned by the users list).</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The account is deleted and its data erased by every module.</response>
    /// <response code="403">The signed-in person cannot delete this account (for example only an Admin can delete a board or admin account).</response>
    /// <response code="404">No account with this identifier.</response>
    [HttpDelete]
    [EndpointSummary("Deletes an account")]
    [EndpointDescription("Permanent deletion by the board (or by an Admin for board and admin accounts). The person is warned by e-mail; every module erases their data.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAccount(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new DeleteAccountRequest(id), ct)).ToNoContent();
}
