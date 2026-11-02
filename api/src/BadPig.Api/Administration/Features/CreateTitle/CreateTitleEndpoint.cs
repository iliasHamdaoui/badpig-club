using BadPig.Administration.Core;
using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.CreateTitle;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/titles")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CreateTitleEndpoint(IHandler<CreateTitleRequest, CreatedResponse> handler) : ControllerBase
{
    /// <response code="201">The title is created: its identifier is returned.</response>
    /// <response code="400">Invalid title name (1 to 60 characters).</response>
    /// <response code="409">A title with this name already exists.</response>
    [HttpPost]
    [EndpointSummary("Creates a title")]
    [EndpointDescription("Names are unique (case-insensitive).")]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateTitleRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToCreated();
}
