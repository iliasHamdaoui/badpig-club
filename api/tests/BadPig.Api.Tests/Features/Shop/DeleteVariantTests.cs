using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class DeleteVariantTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).DeleteAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_VariantDoesNotExist()
    {
        var response = await As(GivenBoardMember()).DeleteAsync($"{V1}/admin/shop/variants/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_AnUnpaidOrderHoldsItsStock()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board);
        await GivenOrderAsync(adherent, (variantId, 1));

        var response = await As(board).DeleteAsync($"{V1}/admin/shop/variants/{variantId}", CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_DeleteTheVariantAndKeepThePaidOrder_When_NoUnpaidOrderHoldsItsStock()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board);
        var orderId = await GivenOrderAsync(adherent, (variantId, 1));
        await GivenTheOrderIsPaidAsync(adherent, orderId);

        var response = await As(board).DeleteAsync($"{V1}/admin/shop/variants/{variantId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty((await ProductsAsync(board)).Single().Variants);
        Assert.Equal("Tube de volants", (await OrderAsync(adherent, orderId)).Lines.Single().ProductName);
    }
}
