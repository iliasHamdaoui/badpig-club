using System.Text;

namespace BadPig.Orders.Core;

/// <summary>Texts (French) of the e-mails of the shop. The greeting and the signature are added by the e-mail sender.</summary>
public static class OrderNotifications
{
    public static string PaidSubject => "Votre commande est payée";

    public static string BoardSubject(string buyerName) => $"Nouvelle commande de {OneLine(buyerName)}";

    public static string BoardBody(string buyerName, IEnumerable<OrderLine> lines, int totalCents)
    {
        var text = new StringBuilder($"{buyerName} a passé et payé une commande :\n\n");
        foreach (var line in lines)
            text.AppendLine($"- {line.Quantity} × {line.ProductName} ({line.VariantLabel}) : {Money.Format(line.UnitPriceCents * line.Quantity)}");

        text.Append($"\nTotal : {Money.Format(totalCents)}\n\nElle est à remettre à l'adhérent.");
        return text.ToString();
    }

    // A name goes in the subject: no line break.
    private static string OneLine(string text) => text.Replace('\r', ' ').Replace('\n', ' ');

    public static string PaidBody(IEnumerable<OrderLine> lines, int totalCents, string? link)
    {
        var text = new StringBuilder("Nous avons bien reçu le paiement de votre commande :\n\n");
        foreach (var line in lines)
            text.AppendLine($"- {line.Quantity} × {line.ProductName} ({line.VariantLabel}) : {Money.Format(line.UnitPriceCents * line.Quantity)}");

        text.Append($"\nTotal : {Money.Format(totalCents)}\n\nMerci pour votre achat.");
        if (link is not null)
            text.Append($"\n\nVoir la commande : {link}");

        return text.ToString();
    }
}
