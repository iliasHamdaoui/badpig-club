using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Shop.Features.SetProductImage;

internal sealed class SetProductImageValidator : AbstractValidator<SetProductImageRequest>
{
    public const int MaxBytes = 3 * 1024 * 1024;
    private static readonly string[] Extensions = [".jpg", ".jpeg", ".png", ".webp"];

    public SetProductImageValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.File).NotNull().WithMessage("Le fichier est obligatoire.")
            .Must(file => file is null || file.Length > 0).WithMessage("Le fichier est vide.")
            .Must(file => file is null || file.Length <= MaxBytes).WithMessage("L'image est trop lourde (3 Mo au plus).")
            .Must(file => file is null || Extensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant())).WithMessage("L'image doit être au format JPEG, PNG ou WebP.");
    }
}
