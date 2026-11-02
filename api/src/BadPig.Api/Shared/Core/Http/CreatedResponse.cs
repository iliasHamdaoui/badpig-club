namespace BadPig.Shared.Core;

/// <summary>Answer (201) of every creation: only the identifier of the new resource.</summary>
/// <param name="Id">Identifier of the created resource (season, title, invited account...), to use in the routes that target it.</param>
public sealed record CreatedResponse(string Id);
