namespace BadPig.Shop.Core;

/// <summary>Settings of the shop (section "Shop").</summary>
public sealed class ShopOptions
{
    public const string Section = "Shop";

    /// <summary>Address that receives the e-mails to the board (low stock). Empty: every active board member receives them.</summary>
    public string NotificationEmail { get; set; } = "";
}
