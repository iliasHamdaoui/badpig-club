using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Profile.Features.GetMemberPhoto;

[ApiController]
[Tags("Profile")]
[Route(ApiRoutes.V1 + "/members/{id}/photo")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class GetMemberPhotoEndpoint(IHandler<GetMemberPhotoRequest, GetMemberPhotoResponse> handler) : ControllerBase
{
    /// <param name="id">Account identifier of the person.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">The picture (JPEG, PNG or WebP).</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    /// <response code="404">This person has no profile picture.</response>
    [HttpGet]
    [EndpointSummary("Downloads the profile picture of a person")]
    [EndpointDescription("Reserved to the members with an up-to-date membership (and the board); a visitor never sees a picture. The address carries the version of the picture (?v=), so the browser may keep it.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPhoto(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new GetMemberPhotoRequest(id), ct)).ToActionResult(photo =>
        {
            Response.Headers.CacheControl = "private, max-age=86400";
            return File(photo.Content, photo.ContentType);
        });
}
