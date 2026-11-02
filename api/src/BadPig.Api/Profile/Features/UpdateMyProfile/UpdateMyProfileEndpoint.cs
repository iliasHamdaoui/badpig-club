using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Profile.Features.UpdateMyProfile;

[ApiController]
[Tags("Profile")]
[Route(ApiRoutes.V1 + "/me")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class UpdateMyProfileEndpoint(IHandler<UpdateMyProfileRequest> handler) : ControllerBase
{
    /// <response code="204">Your profile is updated (read it again with the profile endpoint).</response>
    /// <response code="400">Invalid name or e-mail address.</response>
    /// <response code="404">Your account no longer exists in the identity provider.</response>
    [HttpPut]
    [EndpointSummary("Updates my profile")]
    [EndpointDescription("The target account comes from the token, never from the request.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(UpdateMyProfileRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToNoContent();
}
