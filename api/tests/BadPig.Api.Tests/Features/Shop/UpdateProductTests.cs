using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class UpdateProductTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync($"{V1}/admin/shop/products/{Guid.NewGuid()}", new { name = "Maillot", description = "", isActive = true }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).PutAsJsonAsync($"{V1}/admin/shop/products/{Guid.NewGuid()}", new { name = "Maillot", description = "", isActive = true }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_ArticleDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PutAsJsonAsync($"{V1}/admin/shop/products/{Guid.NewGuid()}", new { name = "Maillot", description = "", isActive = true }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_NameIsMissing()
    {
        var board = GivenBoardMember();
        var id = await GivenProductAsync(board);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/shop/products/{id}", new { name = "", description = "", isActive = true }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ChangeTheArticle_When_BoardMemberUpdatesIt()
    {
        var board = GivenBoardMember();
        var id = await GivenProductAsync(board);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/shop/products/{id}", new { name = "Maillot du club", description = "Maillot technique", isActive = false }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var product = Assert.Single(await ProductsAsync(board));
        Assert.Equal("Maillot du club", product.Name);
        Assert.Equal("Maillot technique", product.Description);
        Assert.False(product.IsActive);
    }
}
