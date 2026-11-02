using BadPig.Shared.Core;

namespace BadPig.Administration.Features.ListBoardMembers;

/// <summary>The board of the club: search (name or title), sort (name, title; default name) and pagination.</summary>
public sealed record ListBoardMembersRequest : ListQuery;
