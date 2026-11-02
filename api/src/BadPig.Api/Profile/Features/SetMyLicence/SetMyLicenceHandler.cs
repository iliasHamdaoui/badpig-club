using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using FluentValidation;

namespace BadPig.Profile.Features.SetMyLicence;

/// <summary>Records OWN licence number: the account comes from the token, never from the request.</summary>
internal sealed class SetMyLicenceHandler(IValidator<SetMyLicenceRequest> validator, IUserContext currentUser,
    IIdentityProvider identity, ILicenceBook licences)
    : IHandler<SetMyLicenceRequest>
{
    public async Task<Result> HandleAsync(SetMyLicenceRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var sub = currentUser.AccountId;
        var me = sub is null ? null : await identity.FindAsync(sub, ct);
        if (me is null)
            return Error.NotFound();

        if (await licences.IsTakenAsync(request.Number, me.Id, ct))
            return Error.Conflict("Ce numéro de licence est déjà celui d'un autre adhérent.");

        await licences.SetAsync(me.Id, request.Number, ct);
        return Result.Success();
    }
}
