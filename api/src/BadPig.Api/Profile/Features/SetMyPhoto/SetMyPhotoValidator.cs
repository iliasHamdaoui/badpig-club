using FluentValidation;

namespace BadPig.Profile.Features.SetMyPhoto;

internal sealed class SetMyPhotoValidator : AbstractValidator<SetMyPhotoRequest>
{
    public const int MaxBytes = 1024 * 1024;
    private static readonly string[] Extensions = [".jpg", ".jpeg", ".png", ".webp"];

    public SetMyPhotoValidator()
    {
        RuleFor(x => x.File).NotNull().WithMessage("Le fichier est obligatoire.")
            .Must(file => file is null || file.Length > 0).WithMessage("Le fichier est vide.")
            .Must(file => file is null || file.Length <= MaxBytes).WithMessage("La photo est trop lourde (1 Mo au plus).")
            .Must(file => file is null || Extensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant())).WithMessage("La photo doit être au format JPEG, PNG ou WebP.");
    }
}
