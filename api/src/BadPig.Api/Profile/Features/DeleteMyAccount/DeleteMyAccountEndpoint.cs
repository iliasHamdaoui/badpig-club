using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Profile.Features.DeleteMyAccount;

[ApiController]
[Tags("Profile")]
[Route(ApiRoutes.V1 + "/me")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class DeleteMyAccountEndpoint(IHandler<DeleteMyAccountRequest> handler) : ControllerBase
{
    /// <response code="204">Your account is deleted.</response>
    /// <response code="404">Your account no longer exists in the identity provider.</response>
    /// <response code="409">You are the last active administrator: appoint another one first.</response>
    [HttpDelete]
    [EndpointSummary("Deletes my own account")]
    [EndpointDescription("Permanent. The last active administrator cannot delete themselves.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteMyAccount(CancellationToken ct) =>
        (await handler.HandleAsync(new DeleteMyAccountRequest(), ct)).ToNoContent();
}
