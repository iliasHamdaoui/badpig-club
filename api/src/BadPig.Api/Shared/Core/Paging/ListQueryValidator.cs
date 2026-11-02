using FluentValidation;

namespace BadPig.Shared.Core;

/// <summary>
/// Rules of the query string common to every list (search, sort, page). A list feature has its own validator that inherits
/// from this one and adds the rule on the sort fields it accepts (see <see cref="SortFieldRules"/>).
/// </summary>
public abstract class ListQueryValidator<TQuery> : AbstractValidator<TQuery> where TQuery : ListQuery
{
    protected ListQueryValidator()
    {
        RuleFor(x => x.Search).Must(search => search is null || search.Length <= 100).WithMessage("La recherche est trop longue (100 caractères au plus).");
        RuleFor(x => x.Page).Must(p => p is null or >= 1).WithMessage("Le numéro de page commence à 1.");
        RuleFor(x => x.PageSize).Must(p => p is null or (>= 1 and <= Paging.MaxPageSize))
            .WithMessage($"La taille de page est comprise entre 1 et {Paging.MaxPageSize}.");
    }
}
