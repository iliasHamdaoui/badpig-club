using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class ListProductsTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync($"{V1}/admin/shop/products", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).GetAsync($"{V1}/admin/shop/products", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_SortIsUnknown()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{V1}/admin/shop/products?sort=price", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ShowTheVariantsWithTheirStock_When_AnUnpaidOrderHoldsSomeUnits()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, priceCents: 1800, quantity: 10);
        await GivenOrderAsync(adherent, (variantId, 3));

        var variant = await VariantAsync(board, variantId);

        Assert.Equal("Vitesse 77", variant.Label);
        Assert.Equal(1800, variant.PriceCents);
        Assert.Equal(10, variant.Quantity);
        Assert.Equal(3, variant.Reserved);
        Assert.Equal(7, variant.Available);
    }

    [Fact]
    public async Task Should_KeepOnlyTheArticlesAtTheirAlertThreshold_When_FilteringOnLowStock()
    {
        var board = GivenBoardMember();
        await GivenAnArticleAsync(board, "Tube de volants", quantity: 3, lowStockThreshold: 5);
        await GivenAnArticleAsync(board, "Maillot du club", "M - Homme", quantity: 20, lowStockThreshold: 5);

        var products = await ProductsAsync(board, "?lowStock=true");

        var product = Assert.Single(products);
        Assert.Equal("Tube de volants", product.Name);
        Assert.True(product.HasLowStock);
        Assert.True(product.Variants.Single().IsLowStock);
    }

    [Fact]
    public async Task Should_FindTheArticle_When_SearchingItsNameOrAVariantLabel()
    {
        var board = GivenBoardMember();
        await GivenAnArticleAsync(board, "Tube de volants", "Vitesse 77");
        await GivenAnArticleAsync(board, "Maillot du club", "M - Femme");

        Assert.Equal("Maillot du club", Assert.Single(await ProductsAsync(board, "?search=maillot")).Name);
        Assert.Equal("Tube de volants", Assert.Single(await ProductsAsync(board, "?search=vitesse")).Name);
    }

    [Fact]
    public async Task Should_ListTheHiddenArticlesToo_When_TheBoardReadsTheList()
    {
        var board = GivenBoardMember();
        var id = await GivenProductAsync(board);
        await As(board).PutAsJsonAsync($"{V1}/admin/shop/products/{id}", new { name = "Tube de volants", description = "", isActive = false }, CancellationToken);

        Assert.False(Assert.Single(await ProductsAsync(board)).IsActive);
    }
}
