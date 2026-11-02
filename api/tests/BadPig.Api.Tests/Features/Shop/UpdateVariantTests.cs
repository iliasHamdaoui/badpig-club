using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class UpdateVariantTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static object Body(string label = "Vitesse 78", int priceCents = 2100, int threshold = 4, bool isActive = true) =>
        new { label, priceCents, lowStockThreshold = threshold, isActive };

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}", Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).PutAsJsonAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}", Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_VariantDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PutAsJsonAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}", Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_PriceIsTooHigh()
    {
        var board = GivenBoardMember();
        var variantId = await GivenAnArticleAsync(board);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/shop/variants/{variantId}", Body(priceCents: 5_000_000), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_AnotherVariantOfTheArticleHasThisLabel()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board);
        await GivenVariantAsync(board, productId, "Vitesse 76");
        var variantId = await GivenVariantAsync(board, productId, "Vitesse 77");

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/shop/variants/{variantId}", Body("Vitesse 76"), CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_ChangeTheVariant_When_BoardMemberUpdatesIt()
    {
        var board = GivenBoardMember();
        var variantId = await GivenAnArticleAsync(board, quantity: 10);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/shop/variants/{variantId}", Body(isActive: false), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var variant = await VariantAsync(board, variantId);
        Assert.Equal(("Vitesse 78", 2100, 4, false, 10), (variant.Label, variant.PriceCents, variant.LowStockThreshold, variant.IsActive, variant.Quantity));
    }

    [Fact]
    public async Task Should_KeepThePriceOfTheOrder_When_ThePriceChangesAfterwards()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, priceCents: 2000);
        var orderId = await GivenOrderAsync(adherent, (variantId, 2));

        await As(board).PutAsJsonAsync($"{V1}/admin/shop/variants/{variantId}", Body(priceCents: 2500), CancellationToken);

        var order = await OrderAsync(adherent, orderId);
        Assert.Equal(4000, order.TotalCents);
        Assert.Equal(2000, order.Lines.Single().UnitPriceCents);
    }
}
