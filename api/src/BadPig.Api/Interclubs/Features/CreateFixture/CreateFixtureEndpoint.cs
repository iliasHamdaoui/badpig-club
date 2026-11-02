using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.CreateFixture;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/teams/{id}/fixtures")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CreateFixtureEndpoint(IHandler<CreateFixtureRequest, CreatedResponse> handler) : ControllerBase
{
    /// <param name="id">Identifier of the team.</param>
    /// <param name="body">The fixture.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="201">The fixture is created: its identifier is returned.</response>
    /// <response code="400">Invalid identifier, date, opponent or venue (see the errors).</response>
    /// <response code="403">The person is neither the captain of this team nor on the board.</response>
    /// <response code="404">No team with this identifier.</response>
    [HttpPost]
    [EndpointSummary("Adds a fixture to the calendar")]
    [EndpointDescription("The captain of the team (or the board) defines a fixture: date, opponent club, at home or away. Several fixtures can be created for the same date.")]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateFixture(string id, CreateFixtureBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new CreateFixtureRequest(id, body), ct)).ToCreated();
}
