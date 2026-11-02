using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class ListShopAuditLogTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/admin/shop/audit-log";

    private async Task<List<ShopAuditEntryDto>> LogAsync(Person board, string query = "") =>
        (await ReadAsync<PageDto<ShopAuditEntryDto>>(await As(board).GetAsync(Route + query, CancellationToken))).Items;

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync(Route, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).GetAsync(Route, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_SortFieldIsUnknown()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{Route}?sort=unknown", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_RecordTheCreation_When_AnArticleAndItsVariantAreCreated()
    {
        var board = GivenBoardMember("Paul Bureau");

        await GivenAnArticleAsync(board, name: "Maillot Club", label: "Femme XS", priceCents: 2000, quantity: 5, lowStockThreshold: 2);

        var log = await LogAsync(board);
        Assert.Contains(log, e => e.Action == "product.created" && e.Actor == "Paul Bureau" && e.Subject == "Maillot Club");
        var variant = Assert.Single(log, e => e.Action == "variant.created");
        Assert.Equal("Maillot Club – Femme XS", variant.Subject);
        Assert.Contains("20,00 €", variant.Details);
        Assert.Contains("stock initial 5", variant.Details);
    }

    [Fact]
    public async Task Should_RecordTheChanges_When_AnArticleAndAVariantAreUpdated()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board, "Maillot Club");
        var variantId = await GivenVariantAsync(board, productId, "Femme XS", priceCents: 2000, quantity: 5);

        await As(board).PutAsJsonAsync($"{V1}/admin/shop/products/{productId}", new { name = "Maillot du club", description = "", isActive = true }, CancellationToken);
        await As(board).PutAsJsonAsync($"{V1}/admin/shop/variants/{variantId}", new { label = "Femme XS", priceCents = 2200, lowStockThreshold = 0, isActive = true }, CancellationToken);

        var log = await LogAsync(board);
        var product = Assert.Single(log, e => e.Action == "product.updated");
        Assert.Equal("Maillot du club", product.Subject);
        Assert.Contains("Maillot Club → Maillot du club", product.Details);
        var variant = Assert.Single(log, e => e.Action == "variant.updated");
        Assert.Contains("20,00 € → 22,00 €", variant.Details);
    }

    [Fact]
    public async Task Should_RecordTheRestockAndTheCorrection_When_TheStockChanges()
    {
        var board = GivenBoardMember("Paul Bureau");
        var variantId = await GivenAnArticleAsync(board, name: "Tube de volants", label: "Vitesse 77", quantity: 10);

        await As(board).PostAsJsonAsync($"{V1}/admin/shop/variants/{variantId}/restock", new { quantity = 12, note = "Livraison" }, CancellationToken);
        await As(board).PostAsJsonAsync($"{V1}/admin/shop/variants/{variantId}/stock-correction", new { quantity = 20, note = "Inventaire" }, CancellationToken);

        var log = await LogAsync(board);
        var restock = Assert.Single(log, e => e.Action == "variant.restocked");
        Assert.Equal("Tube de volants – Vitesse 77", restock.Subject);
        Assert.Contains("+12", restock.Details);
        Assert.Contains("Livraison", restock.Details);
        var correction = Assert.Single(log, e => e.Action == "variant.stock_corrected");
        Assert.Contains("22 → 20", correction.Details);
        Assert.Contains("Inventaire", correction.Details);
    }

    [Fact]
    public async Task Should_RecordTheDeletions_When_AVariantAndAnArticleAreDeleted()
    {
        var board = GivenBoardMember();
        var productId = await GivenProductAsync(board, "Maillot Club");
        var variantId = await GivenVariantAsync(board, productId, "Homme M", quantity: 3);

        await As(board).DeleteAsync($"{V1}/admin/shop/variants/{variantId}", CancellationToken);
        await As(board).DeleteAsync($"{V1}/admin/shop/products/{productId}", CancellationToken);

        var log = await LogAsync(board);
        Assert.Equal("Maillot Club – Homme M", Assert.Single(log, e => e.Action == "variant.deleted").Subject);
        Assert.Equal("Maillot Club", Assert.Single(log, e => e.Action == "product.deleted").Subject);
    }

    [Fact]
    public async Task Should_RecordTheHandingOver_When_AnOrderIsHandedOver()
    {
        var board = GivenBoardMember("Paul Bureau");
        var adherent = await GivenAnAdherentAsync(board);
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));
        await GivenTheOrderIsPaidAsync(adherent, orderId);

        await As(board).PostAsync($"{V1}/admin/shop/orders/{orderId}/deliver", null, CancellationToken);

        var entry = Assert.Single(await LogAsync(board), e => e.Action == "order.delivered");
        Assert.Equal("Paul Bureau", entry.Actor);
        Assert.Equal(orderId, entry.TargetId);
        Assert.Null(entry.Subject);
    }

    [Fact]
    public async Task Should_NotRecordAnything_When_TheActionFails()
    {
        var board = GivenBoardMember();

        await As(board).PostAsJsonAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}/restock", new { quantity = 5 }, CancellationToken);

        Assert.Empty(await LogAsync(board));
    }

    [Fact]
    public async Task Should_FindTheLines_When_SearchingTheArticle()
    {
        var board = GivenBoardMember();
        await GivenAnArticleAsync(board, name: "Maillot Club", label: "Femme XS");
        await GivenAnArticleAsync(board, name: "Tube de volants", label: "Vitesse 77");

        var log = await LogAsync(board, "?search=maillot");

        Assert.True(log.Count > 0);
        Assert.All(log, e => Assert.Contains("Maillot", e.Subject));
    }

    [Fact]
    public async Task Should_ListTheMostRecentFirst_When_NoSortIsGiven()
    {
        var board = GivenBoardMember();
        var variantId = await GivenAnArticleAsync(board);
        await As(board).PostAsJsonAsync($"{V1}/admin/shop/variants/{variantId}/restock", new { quantity = 1 }, CancellationToken);

        var log = await LogAsync(board);

        Assert.Equal("variant.restocked", log[0].Action);
    }
}
