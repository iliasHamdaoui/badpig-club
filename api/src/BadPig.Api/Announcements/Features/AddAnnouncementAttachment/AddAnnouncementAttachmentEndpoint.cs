using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.AddAnnouncementAttachment;

[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/admin/announcements/{id}/attachments")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class AddAnnouncementAttachmentEndpoint(IHandler<AddAnnouncementAttachmentRequest, CreatedResponse> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="file">The file, sent as a <c>multipart/form-data</c> field named <c>file</c>.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="201">The attachment is added: the identifier of the file is returned.</response>
    /// <response code="400">Missing, empty, too large or not an accepted type (PDF, images, Office/OpenDocument documents, text; 10 MB at most).</response>
    /// <response code="404">No announcement with this identifier.</response>
    /// <response code="409">The announcement already has 10 attachments.</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(AnnouncementFileRules.MaxRequestBytes)]
    [EndpointSummary("Adds an attachment to an announcement")]
    [EndpointDescription("An announcement has 10 attachments at most. Readers download them from the announcement; the type is decided from the file extension.")]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddAttachment(string id, IFormFile? file, CancellationToken ct)
    {
        await using var content = file?.OpenReadStream();
        var upload = file is null ? null : new UploadedFile(file.FileName, file.Length, content!);
        return (await handler.HandleAsync(new AddAnnouncementAttachmentRequest(id, upload), ct)).ToCreated();
    }
}
