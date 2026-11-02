using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class DeleteProductTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{V1}/admin/shop/products/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).DeleteAsync($"{V1}/admin/shop/products/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_ArticleDoesNotExist()
    {
        var response = await As(GivenBoardMember()).DeleteAsync($"{V1}/admin/shop/products/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_AnUnpaidOrderHoldsItsStock()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var productId = await GivenProductAsync(board);
        var variantId = await GivenVariantAsync(board, productId);
        await GivenOrderAsync(adherent, (variantId, 1));

        var response = await As(board).DeleteAsync($"{V1}/admin/shop/products/{productId}", CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Single(await ProductsAsync(board));
    }

    [Fact]
    public async Task Should_DeleteTheArticleAndItsVariants_When_NoOrderHoldsItsStock()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board);
        await GivenVariantAsync(board, productId);

        var response = await As(board).DeleteAsync($"{V1}/admin/shop/products/{productId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await ProductsAsync(board));
    }
}
