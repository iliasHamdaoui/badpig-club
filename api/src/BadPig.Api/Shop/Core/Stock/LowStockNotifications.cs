namespace BadPig.Shop.Core;

/// <summary>Texts (French) of the e-mails of the shop to the board. The greeting and the signature are added by the e-mail sender.</summary>
public static class LowStockNotifications
{
    public static string Subject(string productName, string label) => OneLine($"Stock bas : {productName} ({label})");

    public static string Body(string productName, string label, int available, int threshold) =>
        $"Le stock de « {productName} » ({label}) est bas : il reste {available} unité{(available > 1 ? "s" : "")} disponible{(available > 1 ? "s" : "")} " +
        $"(seuil d'alerte : {threshold}).\n\nPensez à réapprovisionner.";

    // Names go in the subject: no line break.
    private static string OneLine(string text) => text.Replace('\r', ' ').Replace('\n', ' ');
}
