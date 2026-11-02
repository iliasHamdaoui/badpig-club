using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Orders;

public sealed class OrdersAccountErasureTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_KeepThePaidOrderWithoutTheName_When_TheBuyerDeletesTheirAccount()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board, "Marie Adherente");
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        var orderId = await GivenOrderAsync(adherent, (variantId, 2));
        await GivenTheOrderIsPaidAsync(adherent, orderId);

        var response = await As(adherent).DeleteAsync($"{V1}/me", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var orders = await ReadAsync<PageDto<BoardOrderDto>>(await As(board).GetAsync($"{V1}/admin/shop/orders", CancellationToken));
        var order = Assert.Single(orders.Items);
        Assert.Equal(("Compte supprimé", "paid", 4000), (order.BuyerName, order.Status, order.TotalCents));
    }

    [Fact]
    public async Task Should_CancelTheUnpaidOrderAndReleaseItsStock_When_TheBuyerDeletesTheirAccount()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        await GivenOrderAsync(adherent, (variantId, 4));

        var response = await As(adherent).DeleteAsync($"{V1}/me", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(10, (await VariantAsync(board, variantId)).Available);
        var orders = await ReadAsync<PageDto<BoardOrderDto>>(await As(board).GetAsync($"{V1}/admin/shop/orders", CancellationToken));
        Assert.Equal("cancelled", Assert.Single(orders.Items).Status);
    }
}
