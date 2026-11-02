using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Orders;

public sealed class GetMyOrderTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync($"{V1}/shop/orders/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_IdentifierIsInvalid()
    {
        var response = await As(GivenMember()).GetAsync($"{V1}/shop/orders/not-an-id", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TheOrderIsSomeoneElses()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var other = await GivenAnAdherentAsync(board, "Julie Adherente");
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));

        var response = await As(other).GetAsync($"{V1}/shop/orders/{orderId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TheOrderDoesNotExist()
    {
        var response = await As(GivenMember()).GetAsync($"{V1}/shop/orders/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_SellTheStockAndEmailTheBuyer_When_ThePaymentProviderSaysItIsPaid()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        var orderId = await GivenOrderAsync(adherent, (variantId, 2));

        await GivenTheOrderIsPaidAsync(adherent, orderId);

        var order = await OrderAsync(adherent, orderId);
        Assert.Equal("paid", order.Status);
        Assert.NotNull(order.PaidAt);
        Assert.Null(order.CheckoutUrl);
        var variant = await VariantAsync(board, variantId);
        Assert.Equal((8, 0, 8), (variant.Quantity, variant.Reserved, variant.Available));
        _ = Api.Mail.Received(1).SendAsync(Arg.Is<AccountInfo?>(a => a != null && a.Id == adherent.Id), "Votre commande est payée",
            Arg.Is<string>(b => b.Contains("2 × Tube de volants (Vitesse 77) : 40,00 €") && b.Contains("Total : 40,00 €") && !b.Contains("main propre")), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_SettleTheOrderOnce_When_ItIsReadSeveralTimesAfterThePayment()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        var orderId = await GivenOrderAsync(adherent, (variantId, 2));
        await GivenTheOrderIsPaidAsync(adherent, orderId);

        await OrderAsync(adherent, orderId);
        await OrderAsync(adherent, orderId);

        Assert.Equal(8, (await VariantAsync(board, variantId)).Quantity);
        _ = Api.Mail.Received(1).SendAsync(Arg.Any<AccountInfo?>(), "Votre commande est payée", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ExpireTheOrderAndReleaseTheStock_When_ItIsNotPaidInTime()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        var orderId = await GivenOrderAsync(adherent, (variantId, 2));
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 10, 36, 0, TimeSpan.Zero));

        var order = await OrderAsync(adherent, orderId);

        Assert.Equal("expired", order.Status);
        Assert.Null(order.CheckoutUrl);
        Assert.Equal((10, 0), ((await VariantAsync(board, variantId)).Available, (await VariantAsync(board, variantId)).Reserved));
    }

    [Fact]
    public async Task Should_NotLoseThePayment_When_TheBuyerPaidJustBeforeTheEnd()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        var orderId = await GivenOrderAsync(adherent, (variantId, 2));
        await GivenThePaymentPageIsAsync(adherent, orderId, PaymentState.Paid);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 10, 40, 0, TimeSpan.Zero));

        var order = await OrderAsync(adherent, orderId);

        Assert.Equal("paid", order.Status);
        Assert.Equal(8, (await VariantAsync(board, variantId)).Quantity);
    }

    [Fact]
    public async Task Should_ExpireTheOrder_When_ThePaymentPageIsClosed()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        var orderId = await GivenOrderAsync(adherent, (variantId, 2));
        await GivenThePaymentPageIsAsync(adherent, orderId, PaymentState.Expired);

        Assert.Equal("expired", (await OrderAsync(adherent, orderId)).Status);
        Assert.Equal(10, (await VariantAsync(board, variantId)).Available);
    }

    [Fact]
    public async Task Should_StayPending_When_ThePaymentProviderCannotBeAsked()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));
        Api.Payments.GetSessionStateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<PaymentState?>(null));

        var order = await OrderAsync(adherent, orderId);

        Assert.Equal("pending", order.Status);
        Assert.NotNull(order.CheckoutUrl);
    }
}
