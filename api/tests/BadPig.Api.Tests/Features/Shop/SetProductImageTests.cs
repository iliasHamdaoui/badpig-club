using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class SetProductImageTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private async Task<ProductItemDto> ProductAsync(Person board, string productId) =>
        (await ReadAsync<PageDto<ProductItemDto>>(await As(board).GetAsync($"{V1}/admin/shop/products?pageSize=100", CancellationToken))).Items.Single(p => p.Id == productId);

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsync($"{V1}/admin/shop/products/{Guid.NewGuid()}/image", FileForm("a.png", PngBytes()), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).PutAsync($"{V1}/admin/shop/products/{Guid.NewGuid()}/image", FileForm("a.png", PngBytes()), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TheArticleDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PutAsync($"{V1}/admin/shop/products/{Guid.NewGuid()}/image", FileForm("a.png", PngBytes()), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheFileIsNotAnImage()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board);

        var response = await As(board).PutAsync($"{V1}/admin/shop/products/{productId}/image", FileForm("faux.png", System.Text.Encoding.UTF8.GetBytes("<html>not an image</html>")), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheExtensionIsNotAnImageOne()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board);

        var response = await As(board).PutAsync($"{V1}/admin/shop/products/{productId}/image", FileForm("image.gif", PngBytes()), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheImageIsTooLarge()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board);

        var response = await As(board).PutAsync($"{V1}/admin/shop/products/{productId}/image", FileForm("grande.png", PngBytes(3 * 1024 * 1024 + 1)), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_NoFileIsSent()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board);

        var response = await As(board).PutAsync($"{V1}/admin/shop/products/{productId}/image", new MultipartFormDataContent(), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ShowThePictureOfTheArticle_When_TheImageIsValid()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board);

        var response = await As(board).PutAsync($"{V1}/admin/shop/products/{productId}/image", FileForm("maillot.png", PngBytes()), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull((await ProductAsync(board, productId)).ImageVersion);
    }

    [Fact]
    public async Task Should_ChangeTheVersion_When_AnotherImageReplacesIt()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board);
        await As(board).PutAsync($"{V1}/admin/shop/products/{productId}/image", FileForm("a.png", PngBytes()), CancellationToken);
        var first = (await ProductAsync(board, productId)).ImageVersion;
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 16, 10, 0, 0, TimeSpan.Zero));

        await As(board).PutAsync($"{V1}/admin/shop/products/{productId}/image", FileForm("b.png", PngBytes(128)), CancellationToken);

        Assert.NotEqual(first, (await ProductAsync(board, productId)).ImageVersion);
    }

    [Fact]
    public async Task Should_RecordTheChangeInTheShopAuditLog_When_TheImageIsSet()
    {
        var board = GivenBoardMember("Paul Bureau");
        var productId = await GivenProductAsync(board, "Maillot Club");

        await As(board).PutAsync($"{V1}/admin/shop/products/{productId}/image", FileForm("a.png", PngBytes()), CancellationToken);

        var log = (await ReadAsync<PageDto<ShopAuditEntryDto>>(await As(board).GetAsync($"{V1}/admin/shop/audit-log", CancellationToken))).Items;
        Assert.Contains(log, e => e.Action == "product.image_set" && e.Subject == "Maillot Club" && e.Actor == "Paul Bureau");
    }
}
