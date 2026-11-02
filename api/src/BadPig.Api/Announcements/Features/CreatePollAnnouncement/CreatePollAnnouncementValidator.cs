using BadPig.Announcements.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.CreatePollAnnouncement;

internal sealed class CreatePollAnnouncementValidator : AbstractValidator<CreatePollAnnouncementRequest>
{
    public CreatePollAnnouncementValidator()
    {
        RuleFor(x => x.Title).MustBeAnAnnouncementTitle();
        RuleFor(x => x.BodyHtml).Must(body => body is null || body.Length <= AnnouncementValidationRules.MaxBodyLength)
            .WithMessage($"Le contenu est trop long ({AnnouncementValidationRules.MaxBodyLength} caractères au plus).");
        RuleFor(x => x.Kind).MustBeAnAnnouncementKind();
        RuleFor(x => x.Options).NotNull().WithMessage("Les choix sont obligatoires.")
            .Must(HaveAcceptableCount).WithMessage($"Un sondage a de {PollRules.MinOptions} à {PollRules.MaxOptions} choix.")
            .Must(AllHaveALabel).WithMessage($"Chaque choix doit avoir un texte ({PollRules.MaxLabelLength} caractères au plus).")
            .Must(AreAllDifferent).WithMessage("Deux choix ne peuvent pas avoir le même texte.");
        RuleFor(x => x.ClosesOn).NotNull().WithMessage("La date limite du sondage est obligatoire.");
    }

    private static bool HaveAcceptableCount(string[]? options) => options is null || options.Length is >= PollRules.MinOptions and <= PollRules.MaxOptions;

    private static bool AllHaveALabel(string[]? options) =>
        options is null || options.All(o => !string.IsNullOrWhiteSpace(o) && o.Trim().Length <= PollRules.MaxLabelLength);

    private static bool AreAllDifferent(string[]? options) =>
        options is null || options.Select(o => (o ?? "").Trim().ToLowerInvariant()).Distinct().Count() == options.Length;
}
