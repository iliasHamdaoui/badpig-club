using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Profile.Features.RemoveMyPhoto;

[ApiController]
[Tags("Profile")]
[Route(ApiRoutes.V1 + "/me/photo")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class RemoveMyPhotoEndpoint(IHandler<RemoveMyPhotoRequest> handler) : ControllerBase
{
    /// <response code="204">Your profile picture is removed (or you had none).</response>
    [HttpDelete]
    [EndpointSummary("Removes my profile picture")]
    [EndpointDescription("The person can always remove or change their picture. The target account comes from the token, never from the request.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemovePhoto(CancellationToken ct) =>
        (await handler.HandleAsync(new RemoveMyPhotoRequest(), ct)).ToNoContent();
}
