using BadPig.Shared.Core;

namespace BadPig.Administration.Core;

/// <summary>
/// E-mail sent when an account is deleted, by the board (DeleteAccount) or automatically (DeleteDisabledAccounts).
/// The text is in French (it is read by the club members).
/// </summary>
public sealed class AccountDeletedNotification(IEmailSender mail)
{
    public Task SendAsync(AccountInfo person, bool automatic, CancellationToken ct) =>
        mail.SendAsync(person, "Votre compte BadPig a été supprimé",
            (automatic
                ? "Votre compte était désactivé depuis plus d'un mois : il a été supprimé automatiquement."
                : "Votre compte a été supprimé par le bureau du club.") +
            "\n\nVos données personnelles ont été effacées du site. Pour toute question, contactez le bureau du club.", ct);
}
