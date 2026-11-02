using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.ListUsers;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/users")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListUsersEndpoint(IHandler<ListUsersRequest, PagedResponse<ListUsersItem>> handler) : ControllerBase
{
    /// <response code="200">One page of club people.</response>
    /// <response code="400">Invalid sort field.</response>
    [HttpGet]
    [EndpointSummary("Lists the club people")]
    [EndpointDescription("With what the signed-in person can do on each account. Technical accounts stay invisible. Search on name, e-mail, username or licence number; sort on name, email, status or membership; paginated.")]
    [ProducesResponseType<PagedResponse<ListUsersItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromQuery] ListUsersRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToOk();
}
