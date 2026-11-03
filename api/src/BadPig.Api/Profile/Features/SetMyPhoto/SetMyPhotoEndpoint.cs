using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Profile.Features.SetMyPhoto;

[ApiController]
[Tags("Profile")]
[Route(ApiRoutes.V1 + "/me/photo")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class SetMyPhotoEndpoint(IHandler<SetMyPhotoRequest> handler) : ControllerBase
{
    /// <param name="file">The picture, sent as a <c>multipart/form-data</c> field named <c>file</c>.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">Your profile picture is set (it replaces the previous one; read its version again with the profile endpoint).</response>
    /// <response code="400">Missing, empty, too large (1 MB at most) or not an image (JPEG, PNG, WebP).</response>
    /// <response code="409">The picture was changed at the same time: try again.</response>
    [HttpPut]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    [EndpointSummary("Sets my profile picture")]
    [EndpointDescription("The picture is shown next to the comments of the person, in the list of the members and in the club directory. A person has one picture at most: a new one replaces the previous one. The website sends a square picture, centred by the person. The target account comes from the token, never from the request; the type is checked on the content of the file.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetPhoto(IFormFile? file, CancellationToken ct)
    {
        await using var content = file?.OpenReadStream();
        var upload = file is null ? null : new UploadedPhoto(file.FileName, file.Length, content!);
        return (await handler.HandleAsync(new SetMyPhotoRequest(upload), ct)).ToNoContent();
    }
}
