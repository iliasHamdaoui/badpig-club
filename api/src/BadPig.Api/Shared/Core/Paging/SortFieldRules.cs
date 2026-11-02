using FluentValidation;

namespace BadPig.Shared.Core;

public static class SortFieldRules
{
    /// <summary>The sort field is empty (default order of the list) or one of the allowed names (case ignored).</summary>
    public static IRuleBuilderOptions<T, string?> MustBeSortableOn<T>(this IRuleBuilder<T, string?> rule, params string[] allowed) =>
        rule.Must(sort => string.IsNullOrEmpty(sort) || allowed.Contains(sort, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Tri invalide : champs possibles : {string.Join(", ", allowed)}.");
}
