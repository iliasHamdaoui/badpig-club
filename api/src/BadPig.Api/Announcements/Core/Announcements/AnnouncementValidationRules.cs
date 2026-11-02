using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Core;

/// <summary>Validation rules of the content of an announcement (shared by the creation and the update).</summary>
public static class AnnouncementValidationRules
{
    public const int MaxTitleLength = 150;
    public const int MaxBodyLength = 50_000;

    public static IRuleBuilderOptions<T, string?> MustBeAnAnnouncementTitle<T>(this IRuleBuilder<T, string?> rule) =>
        rule.NotEmpty().WithMessage("Le titre est obligatoire.")
            .Must(title => title is null || title.Trim().Length <= MaxTitleLength).WithMessage($"Le titre est trop long ({MaxTitleLength} caractères au plus).");

    public static IRuleBuilderOptions<T, string?> MustBeAnAnnouncementBody<T>(this IRuleBuilder<T, string?> rule) =>
        rule.NotEmpty().WithMessage("Le contenu est obligatoire.")
            .Must(body => body is null || body.Length <= MaxBodyLength).WithMessage($"Le contenu est trop long ({MaxBodyLength} caractères au plus).");

    public static IRuleBuilderOptions<T, string?> MustBeAnAnnouncementKind<T>(this IRuleBuilder<T, string?> rule) =>
        rule.NotEmpty().WithMessage("Le type d'annonce est obligatoire.")
            .Must(kind => kind is null || AnnouncementKinds.IsValid(kind)).WithMessage("Le type d'annonce est « internal » (adhérents) ou « public » (tout visiteur).");
}
