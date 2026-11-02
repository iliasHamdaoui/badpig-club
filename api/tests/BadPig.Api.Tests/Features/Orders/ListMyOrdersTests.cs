using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Orders;

public sealed class ListMyOrdersTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync($"{V1}/shop/orders", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_StatusIsUnknown()
    {
        var response = await As(GivenMember()).GetAsync($"{V1}/shop/orders?status=lost", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ListOnlyMyOrdersMostRecentFirst_When_SeveralPeopleOrdered()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var other = await GivenAnAdherentAsync(board, "Julie Adherente");
        var variantId = await GivenAnArticleAsync(board, quantity: 20);
        var first = await GivenOrderAsync(adherent, (variantId, 2));
        await As(adherent).PostAsync($"{V1}/shop/orders/{first}/cancel", null, CancellationToken);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 10, 5, 0, TimeSpan.Zero));
        var second = await GivenOrderAsync(adherent, (variantId, 3));
        await GivenOrderAsync(other, (variantId, 1));

        var mine = await ReadAsync<PageDto<MyOrderItemDto>>(await As(adherent).GetAsync($"{V1}/shop/orders", CancellationToken));

        Assert.Equal([second, first], mine.Items.Select(o => o.Id).ToArray());
        Assert.Equal((3, 6000, "pending"), (mine.Items[0].ItemCount, mine.Items[0].TotalCents, mine.Items[0].Status));
    }

    [Fact]
    public async Task Should_KeepTheOrdersInThisStatus_When_FilteringOnTheStatus()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 20);
        var cancelled = await GivenOrderAsync(adherent, (variantId, 1));
        await As(adherent).PostAsync($"{V1}/shop/orders/{cancelled}/cancel", null, CancellationToken);
        await GivenOrderAsync(adherent, (variantId, 1));

        var mine = await ReadAsync<PageDto<MyOrderItemDto>>(await As(adherent).GetAsync($"{V1}/shop/orders?status=cancelled", CancellationToken));

        Assert.Equal(cancelled, Assert.Single(mine.Items).Id);
    }
}
