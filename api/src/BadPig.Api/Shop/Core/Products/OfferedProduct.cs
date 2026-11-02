namespace BadPig.Shop.Core;

/// <summary>An article offered to the members, with its offered variants. <paramref name="ImageVersion"/> is null without a picture.</summary>
public sealed record OfferedProduct(Guid Id, string Name, string Description, long? ImageVersion, IReadOnlyList<OfferedVariant> Variants);

/// <summary>An offered variant: <paramref name="Available"/> units can be ordered now.</summary>
public sealed record OfferedVariant(Guid Id, string Label, int PriceCents, int Available);
