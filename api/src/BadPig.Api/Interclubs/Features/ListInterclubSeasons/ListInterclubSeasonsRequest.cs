using BadPig.Shared.Core;

namespace BadPig.Interclubs.Features.ListInterclubSeasons;

/// <summary>Seasons of the interclubs: search (name), sort (name, start; default: the latest first) and pagination.</summary>
public sealed record ListInterclubSeasonsRequest : ListQuery;
