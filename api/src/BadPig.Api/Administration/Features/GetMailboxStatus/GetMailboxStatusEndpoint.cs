using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.GetMailboxStatus;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/mailbox")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class GetMailboxStatusEndpoint(IHandler<GetMailboxStatusRequest, GetMailboxStatusResponse> handler) : ControllerBase
{
    /// <param name="ct">Cancellation of the request.</param>
    /// <response code="200">How much new mail waits, and where to read it.</response>
    [HttpGet]
    [EndpointSummary("Tells whether new mail waits in the club mailbox")]
    [EndpointDescription("Counts the unread messages of the club mailbox, without ever reading them. Answers isAvailable = false when no mailbox is configured or when the mail server cannot be reached, so that the site simply shows nothing. The answer is kept for a minute.")]
    [ProducesResponseType<GetMailboxStatusResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct) =>
        (await handler.HandleAsync(new GetMailboxStatusRequest(), ct)).ToOk();
}
