using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Orders;

public sealed class ListCatalogTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync($"{V1}/shop/products", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_MembershipIsNotValidated()
    {
        var response = await As(GivenMember()).GetAsync($"{V1}/shop/products", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_SortIsUnknown()
    {
        var board = GivenBoardMember();

        var response = await As(board).GetAsync($"{V1}/shop/products?sort=price", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ListTheOfferedArticlesWithTheirPrices_When_AdherentReadsTheCatalogue()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var tubes = await GivenProductAsync(board, "Tube de volants", "12 volants");
        await GivenVariantAsync(board, tubes, "Vitesse 77", 2000, 10);
        await GivenVariantAsync(board, tubes, "Vitesse 78", 2100, 0);
        var shirts = await GivenProductAsync(board, "Maillot du club");
        await GivenVariantAsync(board, shirts, "M - Homme", 2000, 4);

        var catalogue = await CatalogAsync(adherent);

        Assert.Equal(["Maillot du club", "Tube de volants"], catalogue.Select(p => p.Name).ToArray());
        var tube = catalogue.Single(p => p.Name == "Tube de volants");
        Assert.Equal("12 volants", tube.Description);
        Assert.Equal([("Vitesse 77", 2000, 10, true), ("Vitesse 78", 2100, 0, false)], tube.Variants.Select(v => (v.Label, v.PriceCents, v.Available, v.InStock)).ToArray());
    }

    [Fact]
    public async Task Should_HideTheArticlesAndVariantsThatAreNotOffered_When_AdherentReadsTheCatalogue()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var hidden = await GivenProductAsync(board, "Ancien maillot");
        await GivenVariantAsync(board, hidden, "M - Homme");
        await As(board).PutAsJsonAsync($"{V1}/admin/shop/products/{hidden}", new { name = "Ancien maillot", description = "", isActive = false }, CancellationToken);
        var tubes = await GivenProductAsync(board, "Tube de volants");
        await GivenVariantAsync(board, tubes, "Vitesse 77");
        var stopped = await GivenVariantAsync(board, tubes, "Vitesse 75");
        await As(board).PutAsJsonAsync($"{V1}/admin/shop/variants/{stopped}", new { label = "Vitesse 75", priceCents = 2000, lowStockThreshold = 0, isActive = false }, CancellationToken);
        await GivenProductAsync(board, "Article sans variante");

        var catalogue = await CatalogAsync(adherent);

        var tube = Assert.Single(catalogue);
        Assert.Equal("Vitesse 77", Assert.Single(tube.Variants).Label);
    }

    [Fact]
    public async Task Should_NotCountTheUnitsHeldByAnUnpaidOrder_When_ReadingTheCatalogue()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var other = await GivenAnAdherentAsync(board, "Julie Adherente");
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        await GivenOrderAsync(other, (variantId, 4));

        var variant = (await CatalogAsync(adherent)).Single().Variants.Single();

        Assert.Equal(6, variant.Available);
    }

    [Fact]
    public async Task Should_GiveTheUnitsBack_When_TheUnpaidOrderIsTooOld()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var other = await GivenAnAdherentAsync(board, "Julie Adherente");
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        await GivenOrderAsync(other, (variantId, 4));
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 11, 0, 0, TimeSpan.Zero));

        var variant = (await CatalogAsync(adherent)).Single().Variants.Single();

        Assert.Equal(10, variant.Available);
    }

    [Fact]
    public async Task Should_FindTheArticle_When_SearchingItsName()
    {
        var board = GivenBoardMember();
        await GivenAnArticleAsync(board, "Tube de volants");
        await GivenAnArticleAsync(board, "Maillot du club", "M - Homme");

        var found = await ReadAsync<PageDto<CatalogItemDto>>(await As(board).GetAsync($"{V1}/shop/products?search=maillot", CancellationToken));

        Assert.Equal("Maillot du club", Assert.Single(found.Items).Name);
    }
}
