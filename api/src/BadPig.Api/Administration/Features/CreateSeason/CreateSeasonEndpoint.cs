using BadPig.Administration.Core;
using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.CreateSeason;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/seasons")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CreateSeasonEndpoint(IHandler<CreateSeasonRequest, CreatedResponse> handler) : ControllerBase
{
    [HttpPost]
    [EndpointSummary("Creates a season")]
    [EndpointDescription("The board sets the dates and the two yearly fees (club licence holders and external members). Seasons cannot overlap and their names are unique. Defaults: end date one year after the start minus a day, name made of the two years, 30 days of grace period.")]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateSeasonRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToCreated();
}
