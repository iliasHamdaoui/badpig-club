using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;

namespace BadPig.Api.Tests.Features.Shop;

public sealed class LowStockAlertTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    // The low stock e-mails all have "Stock bas" in their subject.
    private void AssertLowStockEmail(Person to, int times, string? subjectContains = null, string? bodyContains = null) =>
        _ = Api.Mail.Received(times).SendAsync(
            Arg.Is<AccountInfo?>(a => a != null && a.Id == to.Id),
            Arg.Is<string>(s => s.StartsWith("Stock bas") && (subjectContains == null || s.Contains(subjectContains))),
            Arg.Is<string>(b => bodyContains == null || b.Contains(bodyContains)),
            Arg.Any<CancellationToken>());

    private async Task PayAsync(Person buyer, string variantId, int quantity) =>
        await GivenTheOrderIsPaidAsync(buyer, await GivenOrderAsync(buyer, (variantId, quantity)));

    [Fact]
    public async Task Should_EmailEveryBoardMember_When_APaidOrderBringsTheStockToItsThreshold()
    {
        var paul = GivenBoardMember("Paul Bureau");
        var alice = GivenAdmin("Alice Admin");
        var adherent = await GivenAnAdherentAsync(paul);
        var variantId = await GivenAnArticleAsync(paul, "Tube de volants", "Vitesse 77", 1900, quantity: 10, lowStockThreshold: 5);

        await PayAsync(adherent, variantId, 5);

        AssertLowStockEmail(paul, 1, "Tube de volants (Vitesse 77)", "il reste 5 unités disponibles (seuil d'alerte : 5)");
        AssertLowStockEmail(alice, 1);
    }

    [Fact]
    public async Task Should_NotEmailAnyone_When_TheStockStaysAboveItsThreshold()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10, lowStockThreshold: 5);

        await PayAsync(adherent, variantId, 4);

        AssertLowStockEmail(board, 0);
    }

    [Fact]
    public async Task Should_NotEmailAnyone_When_TheVariantHasNoThreshold()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 3);

        await PayAsync(adherent, variantId, 3);

        AssertLowStockEmail(board, 0);
    }

    [Fact]
    public async Task Should_NotEmailAnyone_When_TheOrderIsOnlyPlaced()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10, lowStockThreshold: 5);

        await GivenOrderAsync(adherent, (variantId, 8));

        AssertLowStockEmail(board, 0);
    }

    [Fact]
    public async Task Should_EmailOnce_When_TheStockKeepsFallingUnderItsThreshold()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10, lowStockThreshold: 5);

        await PayAsync(adherent, variantId, 6);
        await PayAsync(adherent, variantId, 2);

        AssertLowStockEmail(board, 1);
    }

    [Fact]
    public async Task Should_EmailAgain_When_TheStockIsBackAboveItsThresholdThenFallsAgain()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10, lowStockThreshold: 5);
        await PayAsync(adherent, variantId, 6);
        await As(board).PostAsJsonAsync($"{V1}/admin/shop/variants/{variantId}/restock", new { quantity = 5 }, CancellationToken);

        await PayAsync(adherent, variantId, 5);

        AssertLowStockEmail(board, 2);
    }

    [Fact]
    public async Task Should_EmailAgain_When_ACancelledOrderPutTheStockBackAboveItsThreshold()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var other = await GivenAnAdherentAsync(board, "Julie Adherente");
        var variantId = await GivenAnArticleAsync(board, quantity: 10, lowStockThreshold: 5);
        var held = await GivenOrderAsync(other, (variantId, 2));
        await PayAsync(adherent, variantId, 3);
        await As(other).PostAsync($"{V1}/shop/orders/{held}/cancel", null, CancellationToken);

        await PayAsync(adherent, variantId, 3);

        AssertLowStockEmail(board, 2);
    }

    [Fact]
    public async Task Should_EmailAgain_When_TheBoardLowersTheThresholdThenTheStockFallsUnderIt()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10, lowStockThreshold: 5);
        await PayAsync(adherent, variantId, 6);
        await As(board).PutAsJsonAsync($"{V1}/admin/shop/variants/{variantId}", new { label = "Vitesse 77", priceCents = 2000, lowStockThreshold = 1, isActive = true }, CancellationToken);

        await PayAsync(adherent, variantId, 3);

        AssertLowStockEmail(board, 2);
    }

    [Fact]
    public async Task Should_StillConfirmThePayment_When_TheLowStockEmailCannotBeSent()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10, lowStockThreshold: 5);
        Api.Mail.SendAsync(Arg.Any<AccountInfo?>(), Arg.Is<string>(s => s.StartsWith("Stock bas")), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("mail server down")));
        var orderId = await GivenOrderAsync(adherent, (variantId, 6));

        await GivenTheOrderIsPaidAsync(adherent, orderId);

        Assert.Equal(4, (await VariantAsync(board, variantId)).Quantity);
    }
}
