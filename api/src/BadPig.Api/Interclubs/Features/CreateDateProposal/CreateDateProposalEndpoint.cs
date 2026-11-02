using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.CreateDateProposal;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/teams/{id}/proposals")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CreateDateProposalEndpoint(IHandler<CreateDateProposalRequest, CreatedResponse> handler) : ControllerBase
{
    /// <param name="id">Identifier of the team.</param>
    /// <param name="body">The opponent, the venue and the dates.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="201">The proposal is created: its identifier is returned.</response>
    /// <response code="400">Invalid identifier, opponent, venue or dates: two dates at least, ten at most, all different (see the errors).</response>
    /// <response code="403">The person is neither the captain of this team nor on the board.</response>
    /// <response code="404">No team with this identifier.</response>
    [HttpPost]
    [EndpointSummary("Proposes dates for the next fixture")]
    [EndpointDescription("The captain of the team (or the board) offers two dates or more for the next fixture, with the opponent and the venue, to see who is available. The adherents answer; the captain then chooses a date, which creates the fixture.")]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateDateProposal(string id, CreateDateProposalBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new CreateDateProposalRequest(id, body), ct)).ToCreated();
}
