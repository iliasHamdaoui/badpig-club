using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.CancelMembership;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/users/{id}/membership/cancel")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CancelMembershipEndpoint(IHandler<CancelMembershipRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the account (the <c>id</c> returned by the users list).</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The membership of the current season is cancelled.</response>
    /// <response code="403">The target is the signed-in person: ask another board member.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="404">No account with this identifier.</response>
    /// <response code="409">No season is open.</response>
    [HttpPost]
    [EndpointSummary("Cancels the membership of the current season")]
    [EndpointDescription("Never for oneself.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelMembership(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new CancelMembershipRequest(id), ct)).ToNoContent();
}
