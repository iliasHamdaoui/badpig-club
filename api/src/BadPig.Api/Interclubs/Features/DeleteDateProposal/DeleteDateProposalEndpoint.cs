using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.DeleteDateProposal;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/proposals/{id}")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class DeleteDateProposalEndpoint(IHandler<DeleteDateProposalRequest> handler) : ControllerBase
{
    /// <param name="id">Identifier of the proposal.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The proposal and its answers are deleted.</response>
    /// <response code="400">Invalid identifier.</response>
    /// <response code="403">The person is neither the captain of the team nor on the board.</response>
    /// <response code="404">No proposal with this identifier.</response>
    [HttpDelete]
    [EndpointSummary("Deletes a proposal of dates")]
    [EndpointDescription("The captain of the team (or the board) cancels a proposal and the answers given. A fixture already created from it stays in the calendar.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDateProposal(string id, CancellationToken ct) =>
        (await handler.HandleAsync(new DeleteDateProposalRequest(id), ct)).ToNoContent();
}
