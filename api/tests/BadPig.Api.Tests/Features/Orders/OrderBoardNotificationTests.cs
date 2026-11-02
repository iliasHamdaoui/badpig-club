using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Orders;

public sealed class OrderBoardNotificationTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    // The board's e-mail has "Nouvelle commande" in its subject (the buyer's has another one).
    private void AssertBoardEmail(Person to, int times, string? bodyContains = null) =>
        _ = Api.Mail.Received(times).SendAsync(
            Arg.Is<AccountInfo?>(a => a != null && a.Id == to.Id),
            Arg.Is<string>(s => s.StartsWith("Nouvelle commande de")),
            Arg.Is<string>(b => bodyContains == null || b.Contains(bodyContains)),
            Arg.Any<CancellationToken>());

    [Fact]
    public async Task Should_EmailEveryBoardMemberTheOrder_When_AnOrderIsPaid()
    {
        var paul = GivenBoardMember("Paul Bureau");
        var alice = GivenAdmin("Alice Admin");
        var adherent = await GivenAnAdherentAsync(paul, "Marie Adherente");
        var variantId = await GivenAnArticleAsync(paul, "Tube de volants", "Vitesse 77", 1900, 10);
        var orderId = await GivenOrderAsync(adherent, (variantId, 2));

        await GivenTheOrderIsPaidAsync(adherent, orderId);

        AssertBoardEmail(paul, 1, "Marie Adherente a passé et payé une commande");
        AssertBoardEmail(paul, 1, "2 × Tube de volants (Vitesse 77) : 38,00 €");
        AssertBoardEmail(paul, 1, "Total : 38,00 €");
        AssertBoardEmail(alice, 1);
    }

    [Fact]
    public async Task Should_NotEmailTheBoard_When_TheOrderIsOnlyPlaced()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));

        _ = Api.Mail.DidNotReceive().SendAsync(Arg.Any<AccountInfo?>(), Arg.Is<string>(s => s.StartsWith("Nouvelle commande de")), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_NotEmailTheBoard_When_TheOrderExpires()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 11, 0, 0, TimeSpan.Zero));

        await OrderAsync(adherent, orderId);

        _ = Api.Mail.DidNotReceive().SendAsync(Arg.Any<AccountInfo?>(), Arg.Is<string>(s => s.StartsWith("Nouvelle commande de")), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_EmailOnlyTheBoard_When_AnOrderIsPaid()
    {
        var paul = GivenBoardMember("Paul Bureau");
        var adherent = await GivenAnAdherentAsync(paul, "Marie Adherente");
        var other = await GivenAnAdherentAsync(paul, "Julie Adherente");
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(paul), 1));

        await GivenTheOrderIsPaidAsync(adherent, orderId);

        AssertBoardEmail(paul, 1);
        AssertBoardEmail(other, 0);
        AssertBoardEmail(adherent, 0);
    }

    [Fact]
    public async Task Should_StillEmailTheBuyerAndConfirmThePayment_When_TheBoardEmailCannotBeSent()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));
        Api.Mail.SendAsync(Arg.Any<AccountInfo?>(), Arg.Is<string>(s => s.StartsWith("Nouvelle commande de")), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("mail server down")));

        await GivenTheOrderIsPaidAsync(adherent, orderId);

        _ = Api.Mail.Received(1).SendAsync(Arg.Is<AccountInfo?>(a => a != null && a.Id == adherent.Id), "Votre commande est payée", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_StillEmailTheBoard_When_TheBuyerEmailCannotBeSent()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));
        Api.Mail.SendAsync(Arg.Any<AccountInfo?>(), "Votre commande est payée", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("mail server down")));

        await GivenTheOrderIsPaidAsync(adherent, orderId);

        AssertBoardEmail(board, 1);
    }

    [Fact]
    public async Task Should_EmailTheBoardOnce_When_ThePaymentIsNotifiedTwice()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));
        GivenThePaymentProviderNotifies(SessionIdOf(await OrderAsync(adherent, orderId)), PaymentState.Paid);

        await PostNotificationAsync();
        await PostNotificationAsync();

        AssertBoardEmail(board, 1);
    }
}
