using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Interclubs.Features.ListDateProposals;

[ApiController]
[Tags("Interclubs")]
[Route(ApiRoutes.V1 + "/interclubs/proposals")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListDateProposalsEndpoint(IHandler<ListDateProposalsRequest, PagedResponse<ListDateProposalsItem>> handler) : ControllerBase
{
    /// <param name="request">Filters, search, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of proposals.</response>
    /// <response code="400">Unknown sort, invalid filter or invalid page (see the errors).</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    [HttpGet]
    [EndpointSummary("Lists the proposals of dates")]
    [EndpointDescription("The dates offered by the captains for their next fixtures, with the number of yes, maybe and no per date and the answer of the signed-in person. Reserved to the club members with an up-to-date membership (and the board). Optional filters on a team and on the state (open or chosen); search on the team and opponent names; sort on creation date or team; paginated. Default: the latest first.")]
    [ProducesResponseType<PagedResponse<ListDateProposalsItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListDateProposals([FromQuery] ListDateProposalsRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
