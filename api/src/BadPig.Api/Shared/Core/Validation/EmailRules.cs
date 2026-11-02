using System.Net.Mail;
using FluentValidation;

namespace BadPig.Shared.Core;

public static class EmailRules
{
    /// <summary>The e-mail address (when given: the "required" rule is separate) is valid and at most <paramref name="maxLength"/> characters.</summary>
    public static IRuleBuilderOptions<T, string?> MustBeAnEmail<T>(this IRuleBuilder<T, string?> rule, int maxLength) =>
        rule.Must(email => email is null || IsValid(email.Trim(), maxLength)).WithMessage($"L'adresse e-mail est invalide (au plus {maxLength} caractères).");

    private static bool IsValid(string email, int maxLength) =>
        email.Length <= maxLength && MailAddress.TryCreate(email, out var address) && address.Address == email;
}
