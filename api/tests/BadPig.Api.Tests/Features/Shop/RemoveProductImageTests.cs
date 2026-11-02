using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class RemoveProductImageTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private async Task<ProductItemDto> ProductAsync(Person board, string productId) =>
        (await ReadAsync<PageDto<ProductItemDto>>(await As(board).GetAsync($"{V1}/admin/shop/products?pageSize=100", CancellationToken))).Items.Single(p => p.Id == productId);

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{V1}/admin/shop/products/{Guid.NewGuid()}/image", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).DeleteAsync($"{V1}/admin/shop/products/{Guid.NewGuid()}/image", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TheArticleDoesNotExist()
    {
        var response = await As(GivenBoardMember()).DeleteAsync($"{V1}/admin/shop/products/{Guid.NewGuid()}/image", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ShowTheArticleWithoutPicture_When_TheImageIsRemoved()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board);
        await As(board).PutAsync($"{V1}/admin/shop/products/{productId}/image", FileForm("a.png", PngBytes()), CancellationToken);

        var response = await As(board).DeleteAsync($"{V1}/admin/shop/products/{productId}/image", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null((await ProductAsync(board, productId)).ImageVersion);
    }

    [Fact]
    public async Task Should_ReturnNoContent_When_TheArticleHasNoPicture()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board);

        var response = await As(board).DeleteAsync($"{V1}/admin/shop/products/{productId}/image", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Should_DeleteThePicture_When_TheArticleIsDeleted()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board);
        await As(board).PutAsync($"{V1}/admin/shop/products/{productId}/image", FileForm("a.png", PngBytes()), CancellationToken);

        var response = await As(board).DeleteAsync($"{V1}/admin/shop/products/{productId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
