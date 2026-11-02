namespace BadPig.Shared.Payments;

/// <summary>What the person is asked to pay.</summary>
/// <param name="OrderId">Identifier of the order in the application (given back by the provider with the notifications).</param>
/// <param name="Lines">Articles to pay.</param>
/// <param name="CustomerEmail">E-mail address prefilled on the payment page; null if unknown.</param>
/// <param name="SuccessUrl">Page of the site where the person comes back after paying.</param>
/// <param name="CancelUrl">Page of the site where the person comes back without paying.</param>
/// <param name="ExpiresAt">The payment page closes at this date.</param>
public sealed record PaymentRequest(
    Guid OrderId, IReadOnlyList<PaymentLine> Lines, string? CustomerEmail, string SuccessUrl, string CancelUrl, DateTimeOffset ExpiresAt);

/// <summary>One line of a payment, in euro cents.</summary>
public sealed record PaymentLine(string Name, int UnitPriceCents, int Quantity);
