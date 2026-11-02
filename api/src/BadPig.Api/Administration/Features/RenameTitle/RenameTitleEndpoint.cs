using BadPig.Administration.Core;
using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.RenameTitle;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/titles/{id}")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class RenameTitleEndpoint(IHandler<RenameTitleRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the title (the <c>id</c> returned by the titles list).</param>
    /// <param name="body">JSON body of the request.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The title is renamed.</response>
    /// <response code="400">Invalid title name (1 to 60 characters).</response>
    /// <response code="404">No title with this identifier.</response>
    /// <response code="409">A title with this name already exists.</response>
    [HttpPut]
    [EndpointSummary("Renames a title")]
    [EndpointDescription("Names are unique (case-insensitive).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Rename(string id, RenameTitleBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new RenameTitleRequest(id, body.Label), ct)).ToNoContent();
}
