using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.SetAnnouncementPoster;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/admin/announcements/{id}/poster")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class SetAnnouncementPosterEndpoint(IHandler<SetAnnouncementPosterRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="file">The image, sent as a <c>multipart/form-data</c> field named <c>file</c>.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The poster is set (it replaces the previous one).</response>
    /// <response code="400">Missing, empty, too large or not an image (JPEG, PNG, WebP, GIF; 5 MB at most).</response>
    /// <response code="404">No announcement with this identifier.</response>
    [HttpPut]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(AnnouncementFileRules.MaxRequestBytes)]
    [EndpointSummary("Sets the poster of an announcement")]
    [EndpointDescription("The poster is the picture shown with the announcement. An announcement has one poster at most: a new one replaces the previous one.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPoster(string id, IFormFile? file, CancellationToken ct)
    {
        await using var content = file?.OpenReadStream();
        var upload = file is null ? null : new UploadedFile(file.FileName, file.Length, content!);
        return (await handler.HandleAsync(new SetAnnouncementPosterRequest(id, upload), ct)).ToNoContent();
    }
}
