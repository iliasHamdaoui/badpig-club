namespace BadPig.Shared.Payments;

/// <summary>State of a payment page.</summary>
public enum PaymentState
{
    /// <summary>Waiting for the payment.</summary>
    Open,

    /// <summary>Paid.</summary>
    Paid,

    /// <summary>Closed without payment.</summary>
    Expired,
}
