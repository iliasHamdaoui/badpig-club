using BadPig.Shared.Core;

namespace BadPig.Shared.Payments;

/// <summary>
/// The online payment provider (today Stripe, see this folder). This is the ONLY contract the modules use to take a payment:
/// they never know which product is behind it. A payment is a hosted payment page (the provider shows the card form,
/// Apple Pay, Google Pay...) opened from a link; the provider then tells the application that it was paid.
/// </summary>
public interface IPaymentProvider
{
    /// <summary>Opens a payment page for an order. Fails with 500 when the provider is not configured and 502 when it is unreachable or refuses.</summary>
    Task<Result<PaymentSession>> CreateSessionAsync(PaymentRequest request, CancellationToken ct);

    /// <summary>The state of a payment page; null when the provider cannot be asked.</summary>
    Task<PaymentState?> GetSessionStateAsync(string sessionId, CancellationToken ct);

    /// <summary>Closes a payment page that is still open (best effort: a failure is ignored).</summary>
    Task CloseSessionAsync(string sessionId, CancellationToken ct);

    /// <summary>
    /// Checks a notification sent by the provider (signature) and reads it. Returns a validation error when the signature is wrong;
    /// the success value is null when the notification does not concern a payment page.
    /// </summary>
    Result<PaymentNotification?> ReadNotification(string payload, string? signature);
}
