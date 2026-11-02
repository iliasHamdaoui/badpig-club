using BadPig.Shared.Core;
using BadPig.Orders.Core;
using FluentValidation;

namespace BadPig.Orders.Features.CreateOrder;

internal sealed class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Lines).Must(lines => lines is { Count: >= 1 and <= OrderRules.MaxLines })
            .WithMessage($"Une commande contient de 1 à {OrderRules.MaxLines} articles.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.VariantId).MustBeGuid();
            line.RuleFor(l => l.Quantity).Must(q => q is >= 1 and <= OrderRules.MaxQuantityPerLine)
                .WithMessage($"La quantité est comprise entre 1 et {OrderRules.MaxQuantityPerLine}.");
        });
        RuleFor(x => x.Lines).Must(lines => lines is null || lines.Select(l => l.VariantId?.ToLowerInvariant()).Distinct().Count() == lines.Count)
            .WithMessage("Un article ne peut apparaître qu'une fois : changez sa quantité.");
    }
}
