using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.ResendInvitation;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/users/{id}/resend-invitation")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ResendInvitationEndpoint(IHandler<ResendInvitationRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the account (the <c>id</c> returned by the users list).</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">A new sign-up link is e-mailed.</response>
    /// <response code="403">The signed-in person cannot manage this account.</response>
    /// <response code="404">No account with this identifier.</response>
    /// <response code="409">The person is not pending (they already chose a password).</response>
    /// <response code="500">Invitations are not configured on the server.</response>
    /// <response code="502">The identity provider could not be reached.</response>
    [HttpPost]
    [EndpointSummary("Resends an invitation")]
    [EndpointDescription("New sending of the sign-up link (lost or expired link) for a person who is still pending.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResendInvitation(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new ResendInvitationRequest(id), ct)).ToNoContent();
}
