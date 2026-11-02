using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Profile.Features.GetMyProfile;

[ApiController]
[Tags("Profile")]
[Route(ApiRoutes.V1 + "/me")]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class GetMyProfileEndpoint(IHandler<GetMyProfileRequest, GetMyProfileResponse> handler) : ControllerBase
{
    /// <response code="200">Your profile.</response>
    /// <response code="404">Your account no longer exists in the identity provider.</response>
    [HttpGet]
    [EndpointSummary("Gets my profile")]
    [EndpointDescription("The person is identified by their account identifier (\"sub\"), never by e-mail.")]
    [ProducesResponseType<GetMyProfileResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct) =>
        (await handler.HandleAsync(new GetMyProfileRequest(), ct)).ToOk();
}
