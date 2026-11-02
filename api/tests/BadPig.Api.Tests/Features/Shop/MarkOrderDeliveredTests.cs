using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class MarkOrderDeliveredTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsync($"{V1}/admin/shop/orders/{Guid.NewGuid()}/deliver", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).PostAsync($"{V1}/admin/shop/orders/{Guid.NewGuid()}/deliver", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TheOrderDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PostAsync($"{V1}/admin/shop/orders/{Guid.NewGuid()}/deliver", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_TheOrderIsNotPaid()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));

        var response = await As(board).PostAsync($"{V1}/admin/shop/orders/{orderId}/deliver", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_MarkTheOrderAsDelivered_When_BoardMemberHandsItOver()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));
        await GivenTheOrderIsPaidAsync(adherent, orderId);

        var response = await As(board).PostAsync($"{V1}/admin/shop/orders/{orderId}/deliver", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var order = await OrderAsync(adherent, orderId);
        Assert.Equal("delivered", order.Status);
        Assert.NotNull(order.DeliveredAt);
    }

    [Fact]
    public async Task Should_ShowWhoHandedTheOrderOver_When_TheBoardReadsTheOrders()
    {
        var board = GivenBoardMember("Paul Bureau");
        var adherent = await GivenAnAdherentAsync(board);
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));
        await GivenTheOrderIsPaidAsync(adherent, orderId);
        await As(board).PostAsync($"{V1}/admin/shop/orders/{orderId}/deliver", null, CancellationToken);

        var orders = await ReadAsync<PageDto<BoardOrderDto>>(await As(board).GetAsync($"{V1}/admin/shop/orders?status=delivered", CancellationToken));

        var order = Assert.Single(orders.Items);
        Assert.Equal("Paul Bureau", order.DeliveredByName);
        Assert.NotNull(order.DeliveredAt);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_TheOrderWasAlreadyHandedOver()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));
        await GivenTheOrderIsPaidAsync(adherent, orderId);
        await As(board).PostAsync($"{V1}/admin/shop/orders/{orderId}/deliver", null, CancellationToken);

        var response = await As(board).PostAsync($"{V1}/admin/shop/orders/{orderId}/deliver", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
