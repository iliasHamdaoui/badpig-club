using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Profile.Features.SetMyLicence;

[ApiController]
[Tags("Profile")]
[Route(ApiRoutes.V1 + "/me/licence")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class SetMyLicenceEndpoint(IHandler<SetMyLicenceRequest> handler) : ControllerBase
{
    /// <param name="request">The licence number.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">Your licence number is recorded (read it again with the profile endpoint).</response>
    /// <response code="400">Invalid licence number.</response>
    /// <response code="404">Your account no longer exists in the identity provider.</response>
    [HttpPut]
    [EndpointSummary("Records my licence number")]
    [EndpointDescription("The federation licence number (FFBaD) of the signed-in person; the board can also record it from the list of the members. An empty number removes the one on record. The target account comes from the token, never from the request.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetLicence(SetMyLicenceRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToNoContent();
}
