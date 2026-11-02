using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.UpdateFixture;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/fixtures/{id}")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class UpdateFixtureEndpoint(IHandler<UpdateFixtureRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the fixture.</param>
    /// <param name="body">The new values.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The fixture is changed.</response>
    /// <response code="400">Invalid identifier, date, opponent or venue (see the errors).</response>
    /// <response code="403">The person is neither the captain of the team nor on the board.</response>
    /// <response code="404">No fixture with this identifier.</response>
    [HttpPut]
    [EndpointSummary("Changes a fixture")]
    [EndpointDescription("The captain of the team (or the board) changes the date, time, opponent, venue or place of a fixture.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFixture(string id, UpdateFixtureBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new UpdateFixtureRequest(id, body), ct)).ToNoContent();
}
