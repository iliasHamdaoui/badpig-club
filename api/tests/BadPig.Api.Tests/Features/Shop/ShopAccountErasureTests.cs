using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class ShopAccountErasureTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ForgetTheNameOfTheBoardMemberInTheStockHistory_When_TheirAccountIsDeleted()
    {
        var author = GivenAdmin("Paul Bureau");
        var admin = GivenAdmin("Alice Admin");
        var variantId = await GivenAnArticleAsync(author, quantity: 10);

        await As(author).DeleteAsync($"{V1}/me", CancellationToken);

        var history = await ReadAsync<PageDto<StockMovementDto>>(await As(admin).GetAsync($"{V1}/admin/shop/variants/{variantId}/movements", CancellationToken));
        Assert.Equal("Ancien membre du bureau", Assert.Single(history.Items).AuthorName);
    }

    [Fact]
    public async Task Should_ForgetTheNameOfWhoHandedAnOrderOver_When_TheirAccountIsDeleted()
    {
        var deliverer = GivenAdmin("Paul Bureau");
        var admin = GivenAdmin("Alice Admin");
        var adherent = await GivenAnAdherentAsync(admin);
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(admin, quantity: 10), 1));
        await GivenTheOrderIsPaidAsync(adherent, orderId);
        await As(deliverer).PostAsync($"{V1}/admin/shop/orders/{orderId}/deliver", null, CancellationToken);

        await As(deliverer).DeleteAsync($"{V1}/me", CancellationToken);

        var orders = await ReadAsync<PageDto<BoardOrderDto>>(await As(admin).GetAsync($"{V1}/admin/shop/orders?status=delivered", CancellationToken));
        var order = Assert.Single(orders.Items);
        Assert.Equal("Ancien membre du bureau", order.DeliveredByName);
        Assert.NotNull(order.DeliveredAt);
    }
}
