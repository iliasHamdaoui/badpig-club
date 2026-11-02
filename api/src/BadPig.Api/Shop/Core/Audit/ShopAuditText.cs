using System.Globalization;

namespace BadPig.Shop.Core;

/// <summary>Texts shared by the features that write to the shop audit log.</summary>
public static class ShopAuditText
{
    /// <summary>"Article – variant", the subject of every action on a variant.</summary>
    public static string Variant(string productName, string variantLabel) => $"{productName} – {variantLabel}";

    /// <summary>An amount in euro cents, written for the people: "20,00 €". No "fr-FR" culture: the API runs with invariant globalization.</summary>
    public static string Euros(int cents) => (cents / 100m).ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',') + " €";

    /// <summary>"name: A → B" for each change, or null if nothing changed.</summary>
    public static string? Changes(params (string Field, string? Before, string? After)[] fields)
    {
        var changes = fields.Where(f => f.Before != f.After).Select(f => $"{f.Field} : {f.Before} → {f.After}").ToList();
        return changes.Count == 0 ? null : string.Join(" ; ", changes);
    }
}
