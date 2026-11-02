using BadPig.Shared.Core;

namespace BadPig.Administration.Features.ListUsers;

/// <summary>Search (name, e-mail or username), sort (name, email, status, membership; default name asc) and pagination.</summary>
public sealed record ListUsersRequest : ListQuery;
