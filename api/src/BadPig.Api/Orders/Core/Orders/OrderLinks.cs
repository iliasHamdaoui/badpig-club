namespace BadPig.Orders.Core;

/// <summary>Settings of the shop (section "Shop").</summary>
public sealed class OrderLinks
{
    public const string Section = "Shop";

    /// <summary>Public address of the site (no secret), where the payment provider sends the person back after the payment page.</summary>
    public string SiteUrl { get; set; } = "";

    /// <summary>
    /// Address that receives the e-mail of every paid order (a mailbox or mailing list of the club). Empty: every active board member
    /// receives it, each at their own address.
    /// </summary>
    public string NotificationEmail { get; set; } = "";

    public string? OrderPage(Guid id) => Base is null ? null : $"{Base}/shop/orders/{id}";

    private string? Base => string.IsNullOrWhiteSpace(SiteUrl) ? null : SiteUrl.Trim().TrimEnd('/');
}
