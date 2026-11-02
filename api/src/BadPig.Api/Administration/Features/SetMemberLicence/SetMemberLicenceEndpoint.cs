using BadPig.Shared.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BadPig.Administration.Features.SetMemberLicence;

[ApiController]
[Tags("Administration")]
[Route(ApiRoutes.V1 + "/admin/users/{id}/licence")]
[Authorize(Policy = Policies.Board)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class SetMemberLicenceEndpoint(IHandler<SetMemberLicenceRequest> handler) : ControllerBase
{
    /// <param name="id">Account identifier of the person.</param>
    /// <param name="body">The licence number.</param>
    /// <param name="ct">Cancellation token of the request.</param>
    /// <response code="204">The licence number is recorded.</response>
    /// <response code="400">Invalid identifier or invalid licence number (see the errors).</response>
    /// <response code="404">No club account with this identifier.</response>
    [HttpPut]
    [EndpointSummary("Records the licence number of a person")]
    [EndpointDescription("The federation licence number (FFBaD) of a person of the club, which the person can also record from their own profile. An empty number removes the one on record. The action is written in the journal.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetLicence(string id, SetMemberLicenceBody body, CancellationToken ct) =>
        (await handler.HandleAsync(new SetMemberLicenceRequest(id, body), ct)).ToNoContent();
}
