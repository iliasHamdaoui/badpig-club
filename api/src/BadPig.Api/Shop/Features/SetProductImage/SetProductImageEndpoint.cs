using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Shop.Features.SetProductImage;

[ApiController]
[Tags("Shop")]
[Route(ApiRoutes.V1 + "/admin/shop/products/{id}/image")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class SetProductImageEndpoint(IHandler<SetProductImageRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the article.</param>
    /// <param name="file">The image, sent as a <c>multipart/form-data</c> field named <c>file</c>.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The picture is set (it replaces the previous one).</response>
    /// <response code="400">Missing, empty, too large (3 MB at most) or not an image (JPEG, PNG, WebP).</response>
    /// <response code="404">No article with this identifier.</response>
    /// <response code="409">The article was changed at the same time: try again.</response>
    [HttpPut]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(4 * 1024 * 1024)]
    [EndpointSummary("Sets the picture of an article")]
    [EndpointDescription("The picture is shown with the article in the catalogue. An article has one picture at most: a new one replaces the previous one. The type is checked on the content of the file.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetImage(string id, IFormFile? file, CancellationToken ct)
    {
        await using var content = file?.OpenReadStream();
        var upload = file is null ? null : new UploadedImage(file.FileName, file.Length, content!);
        return (await handler.HandleAsync(new SetProductImageRequest(id, upload), ct)).ToNoContent();
    }
}
