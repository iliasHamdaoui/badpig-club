using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class ListShopOrdersTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync($"{V1}/admin/shop/orders", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).GetAsync($"{V1}/admin/shop/orders", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_StatusIsUnknown()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{V1}/admin/shop/orders?status=lost", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ListTheOrdersOfEveryone_When_TheBoardReadsTheList()
    {
        var board = GivenBoardMember();
        var marie = await GivenAnAdherentAsync(board, "Marie Adherente");
        var julie = await GivenAnAdherentAsync(board, "Julie Adherente");
        var variantId = await GivenAnArticleAsync(board, "Tube de volants", "Vitesse 77", 1900, 20);
        var first = await GivenOrderAsync(marie, (variantId, 2));
        await GivenTheOrderIsPaidAsync(marie, first);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 10, 5, 0, TimeSpan.Zero));
        var second = await GivenOrderAsync(julie, (variantId, 1));

        var orders = await ReadAsync<PageDto<BoardOrderDto>>(await As(board).GetAsync($"{V1}/admin/shop/orders", CancellationToken));

        Assert.Equal([second, first], orders.Items.Select(o => o.Id).ToArray());
        Assert.Equal(("Julie Adherente", "pending", 1900), (orders.Items[0].BuyerName, orders.Items[0].Status, orders.Items[0].TotalCents));
        Assert.Equal(("Marie Adherente", "paid", 3800), (orders.Items[1].BuyerName, orders.Items[1].Status, orders.Items[1].TotalCents));
        Assert.Equal(("Tube de volants", "Vitesse 77", 1900, 2), (orders.Items[1].Lines.Single().ProductName, orders.Items[1].Lines.Single().VariantLabel,
            orders.Items[1].Lines.Single().UnitPriceCents, orders.Items[1].Lines.Single().Quantity));
    }

    [Fact]
    public async Task Should_KeepTheOrdersToHandOver_When_FilteringOnPaid()
    {
        var board = GivenBoardMember();
        var marie = await GivenAnAdherentAsync(board, "Marie Adherente");
        var julie = await GivenAnAdherentAsync(board, "Julie Adherente");
        var variantId = await GivenAnArticleAsync(board, quantity: 20);
        var paid = await GivenOrderAsync(marie, (variantId, 1));
        await GivenTheOrderIsPaidAsync(marie, paid);
        await GivenOrderAsync(julie, (variantId, 1));

        var orders = await ReadAsync<PageDto<BoardOrderDto>>(await As(board).GetAsync($"{V1}/admin/shop/orders?status=paid", CancellationToken));

        Assert.Equal(paid, Assert.Single(orders.Items).Id);
    }

    [Fact]
    public async Task Should_FindTheOrder_When_SearchingTheBuyerName()
    {
        var board = GivenBoardMember();
        var marie = await GivenAnAdherentAsync(board, "Marie Adherente");
        var julie = await GivenAnAdherentAsync(board, "Julie Adherente");
        var variantId = await GivenAnArticleAsync(board, quantity: 20);
        await GivenOrderAsync(marie, (variantId, 1));
        var wanted = await GivenOrderAsync(julie, (variantId, 1));

        var orders = await ReadAsync<PageDto<BoardOrderDto>>(await As(board).GetAsync($"{V1}/admin/shop/orders?search=julie", CancellationToken));

        Assert.Equal(wanted, Assert.Single(orders.Items).Id);
    }

    [Fact]
    public async Task Should_ShowTheOrderAsExpired_When_ItsTimeIsOver()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 12, 0, 0, TimeSpan.Zero));

        var orders = await ReadAsync<PageDto<BoardOrderDto>>(await As(board).GetAsync($"{V1}/admin/shop/orders", CancellationToken));

        Assert.Equal(("expired", orderId), (Assert.Single(orders.Items).Status, orders.Items[0].Id));
    }
}
