using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class CreateProductTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync($"{V1}/admin/shop/products", new { name = "Tube de volants" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).PostAsJsonAsync($"{V1}/admin/shop/products", new { name = "Tube de volants" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_NameIsMissing()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync($"{V1}/admin/shop/products", new { name = " " }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_CreateTheArticle_When_BoardMemberCreatesIt()
    {
        var board = GivenBoardMember();

        var response = await As(board).PostAsJsonAsync($"{V1}/admin/shop/products", new { name = " Tube de volants ", description = "Volants en plumes" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = Assert.Single(await ProductsAsync(board));
        Assert.Equal((await ReadAsync<IdDto>(response)).Id, product.Id);
        Assert.Equal("Tube de volants", product.Name);
        Assert.Equal("Volants en plumes", product.Description);
        Assert.True(product.IsActive);
    }
}
