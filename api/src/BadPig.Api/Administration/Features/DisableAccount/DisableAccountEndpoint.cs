using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.DisableAccount;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/users/{id}/disable")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class DisableAccountEndpoint(IHandler<DisableAccountRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the account (the <c>id</c> returned by the users list).</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The account is disabled; automatic deletion is scheduled in one month (not for board and admin accounts).</response>
    /// <response code="403">The signed-in person cannot disable this account.</response>
    /// <response code="404">No account with this identifier.</response>
    [HttpPost]
    [EndpointSummary("Disables an account")]
    [EndpointDescription("The account can no longer sign in; it is deleted automatically after one month unless re-enabled. Board and admin accounts are never deleted automatically.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableAccount(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new DisableAccountRequest(id), ct)).ToNoContent();
}
