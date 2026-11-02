namespace BadPig.Shared.Payments;

/// <summary>A payment page opened at the provider.</summary>
/// <param name="Id">Identifier of the page at the provider.</param>
/// <param name="Url">Address to send the person to.</param>
public sealed record PaymentSession(string Id, string Url);
