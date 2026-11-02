using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Orders;

public sealed class ReceivePaymentNotificationTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnBadRequest_When_TheSignatureIsWrong()
    {
        var response = await PostNotificationAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheNotificationIsEmpty()
    {
        var response = await Anonymous().PostAsync($"{V1}/shop/payments/notifications", new StringContent("", System.Text.Encoding.UTF8, "application/json"), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNoContentAndChangeNothing_When_ThePaymentPageIsUnknown()
    {
        GivenThePaymentProviderNotifies("cs_test_unknown", PaymentState.Paid);

        var response = await PostNotificationAsync();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        AssertNoPaymentEmailSent();
    }

    [Fact]
    public async Task Should_ReturnNoContentAndChangeNothing_When_TheNotificationDoesNotConcernAPayment()
    {
        Api.Payments.ReadNotification(Arg.Any<string>(), Arg.Any<string?>()).Returns(Result<PaymentNotification?>.Success(null));

        var response = await PostNotificationAsync();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Should_SellTheStockAndEmailTheBuyer_When_ThePaymentIsNotified()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        var orderId = await GivenOrderAsync(adherent, (variantId, 3));
        GivenThePaymentProviderNotifies(SessionIdOf(await OrderAsync(adherent, orderId)), PaymentState.Paid);

        var response = await PostNotificationAsync();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("paid", (await OrderAsync(adherent, orderId)).Status);
        Assert.Equal((7, 0), ((await VariantAsync(board, variantId)).Quantity, (await VariantAsync(board, variantId)).Reserved));
        AssertEmailSentTo(adherent);
    }

    [Fact]
    public async Task Should_SettleTheOrderOnce_When_TheSameNotificationArrivesTwice()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        var orderId = await GivenOrderAsync(adherent, (variantId, 3));
        GivenThePaymentProviderNotifies(SessionIdOf(await OrderAsync(adherent, orderId)), PaymentState.Paid);

        await PostNotificationAsync();
        var second = await PostNotificationAsync();

        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
        Assert.Equal(7, (await VariantAsync(board, variantId)).Quantity);
        _ = Api.Mail.Received(1).SendAsync(Arg.Any<AccountInfo?>(), "Votre commande est payée", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_StillConfirmThePayment_When_TheEmailCannotBeSent()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));
        Api.Mail.SendAsync(Arg.Any<AccountInfo?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("mail server down")));
        GivenThePaymentProviderNotifies(SessionIdOf(await OrderAsync(adherent, orderId)), PaymentState.Paid);

        var response = await PostNotificationAsync();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("paid", (await OrderAsync(adherent, orderId)).Status);
    }

    [Fact]
    public async Task Should_ReleaseTheStock_When_ThePaymentPageExpires()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        var orderId = await GivenOrderAsync(adherent, (variantId, 3));
        GivenThePaymentProviderNotifies(SessionIdOf(await OrderAsync(adherent, orderId)), PaymentState.Expired);

        var response = await PostNotificationAsync();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("expired", (await OrderAsync(adherent, orderId)).Status);
        Assert.Equal(10, (await VariantAsync(board, variantId)).Available);
        AssertNoPaymentEmailSent();
    }

    [Fact]
    public async Task Should_KeepTheCancelledOrderCancelled_When_APaymentIsNotifiedLate()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        var orderId = await GivenOrderAsync(adherent, (variantId, 3));
        var sessionId = SessionIdOf(await OrderAsync(adherent, orderId));
        await As(adherent).PostAsync($"{V1}/shop/orders/{orderId}/cancel", null, CancellationToken);
        GivenThePaymentProviderNotifies(sessionId, PaymentState.Paid);

        var response = await PostNotificationAsync();

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("cancelled", (await OrderAsync(adherent, orderId)).Status);
        Assert.Equal(10, (await VariantAsync(board, variantId)).Quantity);
    }
}
