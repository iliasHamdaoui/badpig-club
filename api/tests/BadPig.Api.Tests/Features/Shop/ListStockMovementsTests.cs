using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class ListStockMovementsTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}/movements", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).GetAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}/movements", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_VariantDoesNotExist()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}/movements", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_SortIsUnknown()
    {
        var board = GivenBoardMember();
        var variantId = await GivenAnArticleAsync(board);

        var response = await As(board).GetAsync($"{V1}/admin/shop/variants/{variantId}/movements?sort=change", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ListTheMostRecentFirst_When_TheStockMovedSeveralTimes()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 11, 0, 0, TimeSpan.Zero));
        await As(board).PostAsJsonAsync($"{V1}/admin/shop/variants/{variantId}/restock", new { quantity = 5, note = "Livraison" }, CancellationToken);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 12, 0, 0, TimeSpan.Zero));
        var orderId = await GivenOrderAsync(adherent, (variantId, 2));
        await GivenTheOrderIsPaidAsync(adherent, orderId);

        var history = await ReadAsync<PageDto<StockMovementDto>>(await As(board).GetAsync($"{V1}/admin/shop/variants/{variantId}/movements", CancellationToken));

        Assert.Equal(["sale", "restock", "restock"], history.Items.Select(m => m.Reason).ToArray());
        Assert.Equal((-2, orderId), (history.Items[0].Change, history.Items[0].OrderId));
    }
}
