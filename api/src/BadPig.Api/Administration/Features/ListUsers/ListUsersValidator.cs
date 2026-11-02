using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.ListUsers;

internal sealed class ListUsersValidator : ListQueryValidator<ListUsersRequest>
{
    public ListUsersValidator()
    {
        RuleFor(x => x.Sort).MustBeSortableOn("name", "email", "status", "membership");
        RuleFor(x => x.Group)
            .Must(group => UserGroupFilters.IsKnown(group))
            .WithMessage($"Groupe inconnu (valeurs possibles : {string.Join(", ", UserGroupFilters.All)}).");
    }
}
