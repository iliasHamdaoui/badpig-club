using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.EnableAccount;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/users/{id}/enable")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class EnableAccountEndpoint(IHandler<EnableAccountRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the account (the <c>id</c> returned by the users list).</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The account is enabled again and the automatic deletion is cancelled.</response>
    /// <response code="403">The signed-in person cannot enable this account.</response>
    /// <response code="404">No account with this identifier.</response>
    [HttpPost]
    [EndpointSummary("Re-enables an account")]
    [EndpointDescription("Cancels the countdown to the automatic deletion.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableAccount(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new EnableAccountRequest(id), ct)).ToNoContent();
}
