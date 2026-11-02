using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Announcements.Features.GetAnnouncementFile;

// No 401 here: the files of the public announcements can be read by visitors who are not signed in.
[ApiController]
[Tags("Announcements")]
[Route(ApiRoutes.V1 + "/announcements/{id}/files/{fileId}")]
[AllowAnonymous]
public sealed class GetAnnouncementFileEndpoint(IHandler<GetAnnouncementFileRequest, GetAnnouncementFileResponse> handler) : ControllerBase
{
    /// <param name="id">Identifier of the announcement.</param>
    /// <param name="fileId">Identifier of the file (listed in the announcement).</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">The content of the file (an image for the poster, a download for an attachment).</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="404">No such file on an announcement that the reader can see.</response>
    [HttpGet]
    [EndpointSummary("Downloads the poster or an attachment of an announcement")]
    [EndpointDescription("Same visibility rules as the announcement itself. The poster is sent to be displayed; an attachment is sent as a download.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFile(string id, string fileId, CancellationToken ct) =>
        (await handler.HandleAsync(new GetAnnouncementFileRequest(id, fileId), ct)).ToActionResult(file =>
            new FileStreamResult(file.Content, file.ContentType) { FileDownloadName = file.IsInline ? null : file.FileName });
}
