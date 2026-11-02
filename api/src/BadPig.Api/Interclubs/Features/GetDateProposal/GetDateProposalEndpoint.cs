using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.GetDateProposal;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/proposals/{id}")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class GetDateProposalEndpoint(IHandler<GetDateProposalRequest, GetDateProposalResponse> handler) : ControllerBase
{
    /// <param name="id">Identifier of the proposal.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">The proposal.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    /// <response code="404">No proposal with this identifier.</response>
    [HttpGet]
    [EndpointSummary("Gets a proposal of dates")]
    [EndpointDescription("One proposal with, for each date, the count of the answers and the answer of the signed-in person. The captain of the team (and the board) also get who answered what. Reserved to the club members with an up-to-date membership (and the board).")]
    [ProducesResponseType<GetDateProposalResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDateProposal(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new GetDateProposalRequest(id), ct)).ToOk();
}
