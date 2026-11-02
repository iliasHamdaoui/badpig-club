using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using FluentValidation;

namespace BadPig.Administration.Features.SetMemberLicence;

/// <summary>
/// The board records the federation licence number of a person. The person can also do it from their own profile:
/// the number is club information, not a right, so the board may record its own as well.
/// </summary>
internal sealed class SetMemberLicenceHandler(IValidator<SetMemberLicenceRequest> validator, IIdentityProvider identity,
    ILicenceBook licences, IAdministrationAudit audit)
    : IHandler<SetMemberLicenceRequest>
{
    public async Task<Result> HandleAsync(SetMemberLicenceRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var target = await identity.FindAsync(request.Id, ct);
        if (target is null || !AccountRules.IsClubMember(target))
            return Error.NotFound();

        if (await licences.IsTakenAsync(request.Body.Number, target.Id, ct))
            return Error.Conflict("Ce numéro de licence est déjà celui d'un autre adhérent.");

        await licences.SetAsync(target.Id, request.Body.Number, ct);
        await audit.RecordAsync("licence.updated", target.Id, ct);
        return Result.Success();
    }
}