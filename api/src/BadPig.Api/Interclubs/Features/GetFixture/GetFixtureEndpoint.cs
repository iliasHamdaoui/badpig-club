using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.GetFixture;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/fixtures/{id}")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class GetFixtureEndpoint(IHandler<GetFixtureRequest, GetFixtureResponse> handler) : ControllerBase
{
    /// <param name="id">Identifier of the fixture.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">The fixture.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    /// <response code="404">No fixture with this identifier.</response>
    [HttpGet]
    [EndpointSummary("Gets a fixture")]
    [EndpointDescription("One fixture with its team and captain. canManage tells whether the signed-in person (captain of the team, or board) can change it. Reserved to the club members with an up-to-date membership (and the board).")]
    [ProducesResponseType<GetFixtureResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFixture(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new GetFixtureRequest(id), ct)).ToOk();
}
