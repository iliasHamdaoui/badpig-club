using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Profile.Features.ListClubDirectory;

[ApiController]
[Tags("Profile")]
[Route(ApiRoutes.V1 + "/club/directory")]
[Authorize(Policy = Policies.Member)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class ListClubDirectoryEndpoint(IHandler<ListClubDirectoryRequest, PagedResponse<ListClubDirectoryItem>> handler) : ControllerBase
{
    /// <param name="query">Search, sort and page.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="200">One page of the people of the directory.</response>
    /// <response code="400">Unknown sort or invalid page (see the errors).</response>
    /// <response code="403">The person is not a club member with an up-to-date membership.</response>
    [HttpGet]
    [EndpointSummary("Lists the club directory")]
    [EndpointDescription("The « trombinoscope »: the active members who put a profile picture, with their name. Reserved to the members with an up-to-date membership (and the board). A member who has not added a picture does not appear. Search on the name; sort on name (default); paginated.")]
    [ProducesResponseType<PagedResponse<ListClubDirectoryItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ListClubDirectory([FromQuery] ListClubDirectoryRequest query, CancellationToken ct) =>
        (await handler.HandleAsync(query, ct)).ToOk();
}
