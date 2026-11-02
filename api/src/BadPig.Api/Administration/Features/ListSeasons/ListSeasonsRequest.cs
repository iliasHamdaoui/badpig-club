using BadPig.Shared.Core;

namespace BadPig.Administration.Features.ListSeasons;

/// <summary>Search (name), sort (startsOn, name; default startsOn desc: most recent first) and pagination.</summary>
public sealed record ListSeasonsRequest : ListQuery;
