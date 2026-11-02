using BadPig.Shared.Core;

namespace BadPig.Administration.Features.ListUsers;

/// <summary>Search (name, e-mail, username or licence), sort (name, email, status, membership; default name asc) and pagination.</summary>
/// <param name="Group">Keeps only one kind of account: <c>members</c> (neither board nor admin), <c>board</c> or <c>admin</c>. Empty: everybody.</param>
public sealed record ListUsersRequest : ListQuery
{
    public string? Group { get; init; }
}
