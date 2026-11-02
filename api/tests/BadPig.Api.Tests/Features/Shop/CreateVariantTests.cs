using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class CreateVariantTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync($"{V1}/admin/shop/products/{Guid.NewGuid()}/variants", new { label = "M - Homme", priceCents = 2000 }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).PostAsJsonAsync($"{V1}/admin/shop/products/{Guid.NewGuid()}/variants", new { label = "M - Homme", priceCents = 2000 }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_ArticleDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync($"{V1}/admin/shop/products/{Guid.NewGuid()}/variants", new { label = "M - Homme", priceCents = 2000 }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_PriceIsZero()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board);

        var response = await As(board).PostAsJsonAsync($"{V1}/admin/shop/products/{productId}/variants", new { label = "M - Homme", priceCents = 0 }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_QuantityIsNegative()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board);

        var response = await As(board).PostAsJsonAsync($"{V1}/admin/shop/products/{productId}/variants", new { label = "M - Homme", priceCents = 2000, quantity = -1 }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_TheArticleAlreadyHasAVariantWithThisLabel()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board);
        await GivenVariantAsync(board, productId, "M - Homme");

        var response = await As(board).PostAsJsonAsync($"{V1}/admin/shop/products/{productId}/variants", new { label = "m - homme", priceCents = 2000 }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_CreateTheVariantWithItsStockInTheHistory_When_AnInitialQuantityIsGiven()
    {
        var board = GivenBoardMember("Paul Bureau");
        var productId = await GivenProductAsync(board, "Maillot du club");

        var response = await As(board).PostAsJsonAsync($"{V1}/admin/shop/products/{productId}/variants", new { label = "M - Homme", priceCents = 2000, quantity = 12, lowStockThreshold = 3 }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = (await ReadAsync<IdDto>(response)).Id;
        var variant = await VariantAsync(board, id);
        Assert.Equal((12, 12, 3), (variant.Quantity, variant.Available, variant.LowStockThreshold));
        var history = await ReadAsync<PageDto<StockMovementDto>>(await As(board).GetAsync($"{V1}/admin/shop/variants/{id}/movements", CancellationToken));
        var movement = Assert.Single(history.Items);
        Assert.Equal((12, "restock", "Paul Bureau"), (movement.Change, movement.Reason, movement.AuthorName));
    }
}
