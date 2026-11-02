using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.InviteUser;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/invitations")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class InviteUserEndpoint(IHandler<InviteUserRequest, CreatedResponse> handler) : ControllerBase
{
    /// <response code="201">The account is created and the sign-up link is e-mailed: the identifier of the account is returned.</response>
    /// <response code="400">Invalid name, e-mail address or group.</response>
    /// <response code="403">Only an Admin can invite an administrator.</response>
    /// <response code="409">This e-mail address is already used by an account.</response>
    /// <response code="500">Invitations are not configured on the server.</response>
    /// <response code="502">The identity provider could not be reached.</response>
    [HttpPost]
    [EndpointSummary("Invites a person")]
    [EndpointDescription("Creates the account (Members group, plus Board for \"board\", plus Admin for \"admin\"), records the kind of membership (internal by default) and e-mails a 3-day link to choose a password. The account is \"pending\" until the first sign-in.")]
    [ProducesResponseType<CreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Invite(InviteUserRequest request, CancellationToken ct) =>
        (await handler.HandleAsync(request, ct)).ToCreated();
}
