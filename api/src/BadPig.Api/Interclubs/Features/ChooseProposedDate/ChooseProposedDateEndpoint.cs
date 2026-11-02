using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.ChooseProposedDate;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/proposals/{id}/choose")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ChooseProposedDateEndpoint(IHandler<ChooseProposedDateRequest, CreatedResponse> handler) : ControllerBase
{
    /// <param name="id">Identifier of the proposal.</param>
    /// <param name="body">The chosen date.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="201">The fixture is created: its identifier is returned.</response>
    /// <response code="400">Invalid identifier, or the date is not one of the proposal (see the errors).</response>
    /// <response code="403">The person is neither the captain of the team nor on the board.</response>
    /// <response code="404">No proposal with this identifier.</response>
    /// <response code="409">A date was already chosen.</response>
    [HttpPost]
    [EndpointSummary("Chooses the date of the fixture")]
    [EndpointDescription("The captain of the team (or the board) chooses one of the proposed dates: the fixture is created in the calendar (with the opponent and venue of the proposal) and the proposal is closed.")]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChooseProposedDate(string id, ChooseProposedDateBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new ChooseProposedDateRequest(id, body), ct)).ToCreated();
}
