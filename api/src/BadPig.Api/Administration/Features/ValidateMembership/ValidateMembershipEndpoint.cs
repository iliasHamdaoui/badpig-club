using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.ValidateMembership;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/users/{id}/membership/validate")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ValidateMembershipEndpoint(IHandler<ValidateMembershipRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the account (the <c>id</c> returned by the users list).</param>
    /// <param name="body">JSON body of the request (the kind of membership is optional).</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The membership of the current season is validated (nothing changes if it already is).</response>
    /// <response code="400">Invalid identifier or kind of membership.</response>
    /// <response code="403">The target is the signed-in person: ask another board member.</response>
    /// <response code="404">No account with this identifier.</response>
    /// <response code="409">The account is disabled, or no season is open.</response>
    [HttpPost]
    [EndpointSummary("Validates the membership of the current season")]
    [EndpointDescription("A board member confirms that the dues of the current season have been paid. The kind (internal or external) decides the fee, copied on the membership. Never for oneself.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ValidateMembership(string id, ValidateMembershipBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new ValidateMembershipRequest(id, body.Kind), ct)).ToNoContent();
}
