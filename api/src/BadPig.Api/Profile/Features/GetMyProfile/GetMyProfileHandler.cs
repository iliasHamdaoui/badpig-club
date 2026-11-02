using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;

namespace BadPig.Profile.Features.GetMyProfile;

/// <summary>
/// Profile of the signed-in person (identified by "sub" = account identifier, never by e-mail), completed with
/// the membership, the board title and the licence number given by the Administration module.
/// </summary>
internal sealed class GetMyProfileHandler(IUserContext currentUser, IIdentityProvider identity, IMembershipReader memberships, IBoardInfo board, ILicenceBook licences)
    : IHandler<GetMyProfileRequest, GetMyProfileResponse>
{
    public async Task<Result<GetMyProfileResponse>> HandleAsync(GetMyProfileRequest request, CancellationToken ct)
    {
        var sub = currentUser.AccountId;
        var account = sub is null ? null : await identity.FindAsync(sub, ct);
        if (account is null)
            return Error.NotFound();

        var membership = await memberships.GetAsync(account.Id, ct);
        // Board and admins keep access without a validated membership; regular members need one.
        var upToDate = membership.State != MembershipStates.ToValidate || AccountRules.IsBoardOrAdmin(account);
        var title = await board.GetTitleAsync(account.Id, ct);
        var licence = await licences.GetAsync(account.Id, ct);
        return new GetMyProfileResponse(account.Id, account.Username, account.Name, account.Email, account.Groups,
            membership.State, upToDate, title?.Label, licence, membership.MemberSince);
    }
}
