using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.CreateTeam;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/admin/interclubs/teams")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CreateTeamEndpoint(IHandler<CreateTeamRequest, CreatedResponse> handler) : ControllerBase
{
    /// <param name="request">The team.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="201">The team is created: its identifier is returned.</response>
    /// <response code="400">Invalid name or season identifier (see the errors).</response>
    /// <response code="404">No season with this identifier.</response>
    /// <response code="409">A team with this name already exists for this season.</response>
    [HttpPost]
    [EndpointSummary("Creates a team")]
    [EndpointDescription("Creates a team of the club in one of its seasons (D6, D5, D4, D1, PR...). Designate its captain afterwards.")]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTeam(CreateTeamRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToCreated();
}
