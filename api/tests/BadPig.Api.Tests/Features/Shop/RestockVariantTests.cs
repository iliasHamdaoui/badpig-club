using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class RestockVariantTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}/restock", new { quantity = 5 }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).PostAsJsonAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}/restock", new { quantity = 5 }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_VariantDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}/restock", new { quantity = 5 }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_QuantityIsZero()
    {
        var board = GivenBoardMember();
        var variantId = await GivenAnArticleAsync(board);

        var response = await As(board).PostAsJsonAsync($"{V1}/admin/shop/variants/{variantId}/restock", new { quantity = 0 }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_AddTheUnitsAndWriteTheHistory_When_BoardMemberRestocks()
    {
        var board = GivenBoardMember("Paul Bureau");
        var variantId = await GivenAnArticleAsync(board, quantity: 10);

        var response = await As(board).PostAsJsonAsync($"{V1}/admin/shop/variants/{variantId}/restock", new { quantity = 24, note = "Livraison fournisseur" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(34, (await VariantAsync(board, variantId)).Quantity);
        var history = await ReadAsync<PageDto<StockMovementDto>>(await As(board).GetAsync($"{V1}/admin/shop/variants/{variantId}/movements", CancellationToken));
        var latest = history.Items.First(m => m.Note == "Livraison fournisseur");
        Assert.Equal((24, "restock", "Paul Bureau"), (latest.Change, latest.Reason, latest.AuthorName));
    }
}
