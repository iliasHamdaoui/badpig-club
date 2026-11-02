using BadPig.Shared.Core;

namespace BadPig.Administration.Core;

/// <summary>E-mail sent when a person loses their title (SetTitle, DeleteTitle). The text is in French.</summary>
public sealed class TitleRemovedNotification(IEmailSender mail)
{
    public Task SendAsync(AccountInfo? person, string title, CancellationToken ct) =>
        mail.SendAsync(person, $"Vous n'êtes plus {title} de BadPig",
            $"Le titre de {title} ne vous est plus attribué. Vous restez membre du bureau.", ct);
}
