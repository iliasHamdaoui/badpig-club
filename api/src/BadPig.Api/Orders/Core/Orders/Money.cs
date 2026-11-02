using System.Globalization;

namespace BadPig.Orders.Core;

/// <summary>Amounts are kept in euro cents; this writes them for the people ("20,00 €").</summary>
public static class Money
{
    // Written by hand on purpose: the API runs with invariant globalization (no ICU), where the "fr-FR" culture does not exist.
    public static string Format(int cents) => (cents / 100m).ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',') + " €";
}
