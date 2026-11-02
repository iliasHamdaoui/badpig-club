using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.DeleteFixture;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/fixtures/{id}")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class DeleteFixtureEndpoint(IHandler<DeleteFixtureRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the fixture.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The fixture is deleted.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="403">The person is neither the captain of the team nor on the board.</response>
    /// <response code="404">No fixture with this identifier.</response>
    [HttpDelete]
    [EndpointSummary("Deletes a fixture")]
    [EndpointDescription("The captain of the team (or the board) removes a fixture from the calendar.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFixture(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new DeleteFixtureRequest(id), ct)).ToNoContent();
}
