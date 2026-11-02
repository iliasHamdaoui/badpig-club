using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class CorrectVariantStockTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}/stock-correction", new { quantity = 5, note = "Inventaire" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).PostAsJsonAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}/stock-correction", new { quantity = 5, note = "Inventaire" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_VariantDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}/stock-correction", new { quantity = 5, note = "Inventaire" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheReasonIsMissing()
    {
        var board = GivenBoardMember();
        var variantId = await GivenAnArticleAsync(board);

        var response = await As(board).PostAsJsonAsync($"{V1}/admin/shop/variants/{variantId}/stock-correction", new { quantity = 5 }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_QuantityIsBelowTheUnitsHeldByUnpaidOrders()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        await GivenOrderAsync(adherent, (variantId, 4));

        var response = await As(board).PostAsJsonAsync($"{V1}/admin/shop/variants/{variantId}/stock-correction", new { quantity = 3, note = "Inventaire" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(10, (await VariantAsync(board, variantId)).Quantity);
    }

    [Fact]
    public async Task Should_SetTheQuantityAndWriteTheDifference_When_BoardMemberCorrectsTheStock()
    {
        var board = GivenBoardMember("Paul Bureau");
        var variantId = await GivenAnArticleAsync(board, quantity: 10);

        var response = await As(board).PostAsJsonAsync($"{V1}/admin/shop/variants/{variantId}/stock-correction", new { quantity = 7, note = "Inventaire de la salle" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(7, (await VariantAsync(board, variantId)).Quantity);
        var history = await ReadAsync<PageDto<StockMovementDto>>(await As(board).GetAsync($"{V1}/admin/shop/variants/{variantId}/movements", CancellationToken));
        var correction = history.Items.Single(m => m.Reason == "correction");
        Assert.Equal((-3, "Inventaire de la salle", "Paul Bureau"), (correction.Change, correction.Note, correction.AuthorName));
    }

    [Fact]
    public async Task Should_WriteNothing_When_TheQuantityWasAlreadyRight()
    {
        var board = GivenBoardMember();
        var variantId = await GivenAnArticleAsync(board, quantity: 10);

        var response = await As(board).PostAsJsonAsync($"{V1}/admin/shop/variants/{variantId}/stock-correction", new { quantity = 10, note = "Inventaire" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var history = await ReadAsync<PageDto<StockMovementDto>>(await As(board).GetAsync($"{V1}/admin/shop/variants/{variantId}/movements", CancellationToken));
        Assert.DoesNotContain(history.Items, m => m.Reason == "correction");
    }
}
