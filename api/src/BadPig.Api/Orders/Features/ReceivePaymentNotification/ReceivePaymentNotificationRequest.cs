namespace BadPig.Orders.Features.ReceivePaymentNotification;

/// <summary>What the payment provider sent: the raw body and its signature.</summary>
/// <param name="Payload">Raw body of the notification (the signature is computed on it).</param>
/// <param name="Signature">Value of the Stripe-Signature header.</param>
public sealed record ReceivePaymentNotificationRequest(string Payload, string? Signature);
