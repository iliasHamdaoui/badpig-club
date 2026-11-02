using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using Microsoft.Extensions.Logging;

namespace BadPig.Administration.Core;

public static class SignupLink
{
    /// <summary>Sends the invitation e-mail (link to choose a password). A failure is reported, never thrown.</summary>
    public static async Task<Result> SendAsync(IIdentityProvider identity, AccountInfo target, ILogger log, CancellationToken ct)
    {
        try
        {
            await identity.SendSignupLinkAsync(target.Id, ct);
            return Result.Success();
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            log.LogError(e, "Invitation e-mail not sent for {Target}", target.Id);
            return Error.BadGateway(
                "Le compte existe, mais l'e-mail n'a pas pu être envoyé. Utilisez « Renvoyer l'invitation » dans la liste.");
        }
    }
}
