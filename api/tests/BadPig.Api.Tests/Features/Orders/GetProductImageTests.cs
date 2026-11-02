using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Orders;

public sealed class GetProductImageTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private async Task<string> GivenAnArticleWithAPictureAsync(Person board, string name = "Maillot Club")
    {
        var productId = await GivenProductAsync(board, name);
        await GivenVariantAsync(board, productId);
        await As(board).PutAsync($"{V1}/admin/shop/products/{productId}/image", FileForm("a.png", PngBytes()), CancellationToken);
        return productId;
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync($"{V1}/shop/products/{Guid.NewGuid()}/image", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_MembershipIsNotValidated()
    {
        var response = await As(GivenMember()).GetAsync($"{V1}/shop/products/{Guid.NewGuid()}/image", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheIdentifierIsInvalid()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{V1}/shop/products/not-a-guid/image", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TheArticleHasNoPicture()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var productId = await GivenProductAsync(board);
        await GivenVariantAsync(board, productId);

        var response = await As(adherent).GetAsync($"{V1}/shop/products/{productId}/image", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_SendThePicture_When_AdherentReadsAnOfferedArticle()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var productId = await GivenAnArticleWithAPictureAsync(board);

        var response = await As(adherent).GetAsync($"{V1}/shop/products/{productId}/image", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(PngBytes(), await response.Content.ReadAsByteArrayAsync(CancellationToken));
    }

    [Fact]
    public async Task Should_ShowTheVersionInTheCatalogue_When_TheArticleHasAPicture()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        await GivenAnArticleWithAPictureAsync(board);

        var article = Assert.Single(await CatalogAsync(adherent));

        Assert.NotNull(article.ImageVersion);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TheArticleIsHiddenFromTheMembers()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var productId = await GivenAnArticleWithAPictureAsync(board);
        await As(board).PutAsJsonAsync($"{V1}/admin/shop/products/{productId}", new { name = "Maillot Club", description = "", isActive = false }, CancellationToken);

        var response = await As(adherent).GetAsync($"{V1}/shop/products/{productId}/image", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_SendThePictureOfAHiddenArticle_When_TheBoardReadsIt()
    {
        var board = GivenBoardMember();
        var productId = await GivenAnArticleWithAPictureAsync(board);
        await As(board).PutAsJsonAsync($"{V1}/admin/shop/products/{productId}", new { name = "Maillot Club", description = "", isActive = false }, CancellationToken);

        var response = await As(board).GetAsync($"{V1}/shop/products/{productId}/image", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
