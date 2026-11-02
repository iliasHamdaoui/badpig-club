using FluentValidation;

namespace BadPig.Announcements.Core;

/// <summary>Validation rules of the files sent with a request (poster and attachments).</summary>
public static class UploadRules
{
    public static IRuleBuilderOptions<T, UploadedFile?> MustBeAPoster<T>(this IRuleBuilder<T, UploadedFile?> rule) =>
        rule.NotNull().WithMessage("Le fichier est obligatoire.")
            .Must(file => file is null || file.Length > 0).WithMessage("Le fichier est vide.")
            .Must(file => file is null || file.Length <= AnnouncementFileRules.MaxPosterBytes).WithMessage("L'affiche est trop lourde (5 Mo au plus).")
            .Must(file => file is null || AnnouncementFileRules.IsImage(file.FileName)).WithMessage("L'affiche doit être une image JPEG, PNG, WebP ou GIF.");

    public static IRuleBuilderOptions<T, UploadedFile?> MustBeAnAttachment<T>(this IRuleBuilder<T, UploadedFile?> rule) =>
        rule.NotNull().WithMessage("Le fichier est obligatoire.")
            .Must(file => file is null || file.Length > 0).WithMessage("Le fichier est vide.")
            .Must(file => file is null || file.Length <= AnnouncementFileRules.MaxAttachmentBytes).WithMessage("La pièce jointe est trop lourde (10 Mo au plus).")
            .Must(file => file is null || AnnouncementFileRules.IsAllowedAttachment(file.FileName))
            .WithMessage("Type de fichier non accepté (PDF, images, documents Office ou OpenDocument, texte).");
}
