using FluentValidation;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;

namespace BadPig.Profile.Features.UpdateMyProfile;

/// <summary>Update of OWN profile: the target account comes from the token, never from the request.</summary>
internal sealed class UpdateMyProfileHandler(IValidator<UpdateMyProfileRequest> validator, IUserContext currentUser, IIdentityProvider identity)
    : IHandler<UpdateMyProfileRequest>
{
    public async Task<Result> HandleAsync(UpdateMyProfileRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var name = request.Name!.Trim();
        var email = request.Email!.Trim();

        var sub = currentUser.AccountId;
        var existing = sub is null ? null : await identity.FindAsync(sub, ct);
        if (existing is null)
            return Error.NotFound();

        await identity.UpdateProfileAsync(existing.Id, name, email, ct);
        return Result.Success();
    }

}
