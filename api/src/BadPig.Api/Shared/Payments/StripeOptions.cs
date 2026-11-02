namespace BadPig.Shared.Payments;

/// <summary>Settings of Stripe (section "Stripe"). The keys are secrets: they live in the server environment, never in Git.</summary>
internal sealed class StripeOptions
{
    public const string Section = "Stripe";

    /// <summary>Secret key (starts with sk_test_ or sk_live_).</summary>
    public string SecretKey { get; set; } = "";

    /// <summary>Signing secret of the webhook (starts with whsec_).</summary>
    public string WebhookSecret { get; set; } = "";

    public string ApiBase { get; set; } = "https://api.stripe.com/v1/";
}
