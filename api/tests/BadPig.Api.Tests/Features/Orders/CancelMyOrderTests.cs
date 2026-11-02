using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Orders;

public sealed class CancelMyOrderTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsync($"{V1}/shop/orders/{Guid.NewGuid()}/cancel", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_IdentifierIsInvalid()
    {
        var response = await As(GivenMember()).PostAsync($"{V1}/shop/orders/not-an-id/cancel", null, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TheOrderIsSomeoneElses()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var other = await GivenAnAdherentAsync(board, "Julie Adherente");
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));

        var response = await As(other).PostAsync($"{V1}/shop/orders/{orderId}/cancel", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_TheOrderIsAlreadyPaid()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));
        await GivenTheOrderIsPaidAsync(adherent, orderId);

        var response = await As(adherent).PostAsync($"{V1}/shop/orders/{orderId}/cancel", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("paid", (await OrderAsync(adherent, orderId)).Status);
    }

    [Fact]
    public async Task Should_ReturnConflictAndSettleThePayment_When_ThePaymentWasJustMade()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        var orderId = await GivenOrderAsync(adherent, (variantId, 2));
        Api.Payments.GetSessionStateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<PaymentState?>(PaymentState.Paid));

        var response = await As(adherent).PostAsync($"{V1}/shop/orders/{orderId}/cancel", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("paid", (await OrderAsync(adherent, orderId)).Status);
        Assert.Equal(8, (await VariantAsync(board, variantId)).Quantity);
    }

    [Fact]
    public async Task Should_ReleaseTheStockAndClosePaymentPage_When_TheBuyerCancelsAnUnpaidOrder()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        var orderId = await GivenOrderAsync(adherent, (variantId, 2));
        var sessionId = SessionIdOf(await OrderAsync(adherent, orderId));

        var response = await As(adherent).PostAsync($"{V1}/shop/orders/{orderId}/cancel", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var order = await OrderAsync(adherent, orderId);
        Assert.Equal("cancelled", order.Status);
        Assert.Null(order.CheckoutUrl);
        Assert.Equal(10, (await VariantAsync(board, variantId)).Available);
        _ = Api.Payments.Received(1).CloseSessionAsync(sessionId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ReturnConflict_When_TheOrderWasAlreadyCancelled()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var orderId = await GivenOrderAsync(adherent, (await GivenAnArticleAsync(board), 1));
        await As(adherent).PostAsync($"{V1}/shop/orders/{orderId}/cancel", null, CancellationToken);

        var response = await As(adherent).PostAsync($"{V1}/shop/orders/{orderId}/cancel", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
