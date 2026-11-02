namespace BadPig.Shared.Payments;

/// <summary>What the provider tells the application: the payment page <paramref name="SessionId"/> is now in this state.</summary>
public sealed record PaymentNotification(string SessionId, PaymentState State);
