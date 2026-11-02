using FluentValidation;

namespace BadPig.Shared.Core;

/// <summary>Validation rules shared by the validators of the features (FluentValidation).</summary>
public static class GuidRules
{
    /// <summary>The value is an identifier (GUID), as used by the accounts of the identity provider and by the entities.</summary>
    public static IRuleBuilderOptions<T, TProperty> MustBeGuid<T, TProperty>(this IRuleBuilder<T, TProperty> rule) =>
        rule.Must(id => Guid.TryParse(id?.ToString(), out _)).WithMessage("Identifiant invalide.");
}
