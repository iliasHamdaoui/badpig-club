using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Features.Orders;

public sealed class CreateOrderTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync($"{V1}/shop/orders", new { lines = new[] { new { variantId = Guid.NewGuid(), quantity = 1 } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_MembershipIsNotValidated()
    {
        var board = GivenBoardMember();
        var variantId = await GivenAnArticleAsync(board);

        var response = await As(GivenMember()).PostAsJsonAsync($"{V1}/shop/orders", new { lines = new[] { new { variantId, quantity = 1 } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheOrderHasNoLine()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/shop/orders", new { lines = Array.Empty<object>() }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_QuantityIsTooHigh()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 100);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/shop/orders", new { lines = new[] { new { variantId, quantity = 11 } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheSameVariantIsOrderedTwice()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/shop/orders", new { lines = new[] { new { variantId, quantity = 1 }, new { variantId, quantity = 2 } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AVariantDoesNotExist()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/shop/orders", new { lines = new[] { new { variantId = Guid.NewGuid(), quantity = 1 } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflictAndHoldNothing_When_OneArticleIsOutOfStock()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var tubes = await GivenAnArticleAsync(board, quantity: 10);
        var shirts = await GivenAnArticleAsync(board, "Maillot du club", "M - Homme", quantity: 1);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/shop/orders", new { lines = new[] { new { variantId = tubes, quantity = 2 }, new { variantId = shirts, quantity = 2 } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(0, (await VariantAsync(board, tubes)).Reserved);
        Assert.Equal(0, (await VariantAsync(board, shirts)).Reserved);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_TheArticleIsNoLongerOffered()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var productId = await GivenProductAsync(board);
        var variantId = await GivenVariantAsync(board, productId);
        await As(board).PutAsJsonAsync($"{V1}/admin/shop/products/{productId}", new { name = "Tube de volants", description = "", isActive = false }, CancellationToken);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/shop/orders", new { lines = new[] { new { variantId, quantity = 1 } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_ThePersonAlreadyHasAnUnpaidOrder()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board);
        await GivenOrderAsync(adherent, (variantId, 1));

        var response = await As(adherent).PostAsJsonAsync($"{V1}/shop/orders", new { lines = new[] { new { variantId, quantity = 1 } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(1, (await VariantAsync(board, variantId)).Reserved);
    }

    [Fact]
    public async Task Should_HoldTheStockAndGiveThePaymentPage_When_AdherentOrders()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board, "Marie Adherente");
        var tubes = await GivenAnArticleAsync(board, "Tube de volants", "Vitesse 77", 1900, 10);
        var shirts = await GivenAnArticleAsync(board, "Maillot du club", "M - Homme", 2000, 5);

        var orderId = await GivenOrderAsync(adherent, (tubes, 2), (shirts, 1));

        var order = await OrderAsync(adherent, orderId);
        Assert.Equal("pending", order.Status);
        Assert.Equal(5800, order.TotalCents);
        Assert.True(order.CheckoutUrl!.StartsWith("https://checkout.test/pay/", StringComparison.Ordinal));
        Assert.Equal(new DateTimeOffset(2026, 10, 15, 10, 35, 0, TimeSpan.Zero), order.ExpiresAt);
        Assert.Equal([("Maillot du club", "M - Homme", 2000, 1), ("Tube de volants", "Vitesse 77", 1900, 2)],
            order.Lines.Select(l => (l.ProductName, l.VariantLabel, l.UnitPriceCents, l.Quantity)).ToArray());
        Assert.Equal(8, (await VariantAsync(board, tubes)).Available);
        Assert.Equal(10, (await VariantAsync(board, tubes)).Quantity);
        Assert.Equal(4, (await VariantAsync(board, shirts)).Available);
    }

    [Fact]
    public async Task Should_AskThePaymentProviderForTheRightAmounts_When_AdherentOrders()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board, "Marie Adherente");
        var tubes = await GivenAnArticleAsync(board, "Tube de volants", "Vitesse 77", 1900, 10);

        var orderId = await GivenOrderAsync(adherent, (tubes, 2));

        _ = Api.Payments.Received(1).CreateSessionAsync(
            Arg.Is<PaymentRequest>(r => r.OrderId.ToString() == orderId
                && r.Lines.Count == 1 && r.Lines[0].Name == "Tube de volants - Vitesse 77" && r.Lines[0].UnitPriceCents == 1900 && r.Lines[0].Quantity == 2
                && r.CustomerEmail == adherent.Email
                && r.SuccessUrl == $"https://test.badpig.fr/shop/orders/{orderId}"
                && r.CancelUrl == $"https://test.badpig.fr/shop/orders/{orderId}?canceled=1"
                && r.ExpiresAt == new DateTimeOffset(2026, 10, 15, 10, 35, 0, TimeSpan.Zero)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_LetTheBoardOrder_When_ABoardMemberOrders()
    {
        var board = GivenBoardMember();
        var variantId = await GivenAnArticleAsync(board);

        var orderId = await GivenOrderAsync(board, (variantId, 1));

        Assert.Equal("pending", (await OrderAsync(board, orderId)).Status);
    }

    [Fact]
    public async Task Should_ReleaseTheStockAndFail_When_ThePaymentProviderIsUnreachable()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        Api.Payments.CreateSessionAsync(Arg.Any<PaymentRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<PaymentSession>>(Error.BadGateway("Le paiement en ligne est indisponible.")));

        var response = await As(adherent).PostAsJsonAsync($"{V1}/shop/orders", new { lines = new[] { new { variantId, quantity = 3 } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal(0, (await VariantAsync(board, variantId)).Reserved);
        var mine = await ReadAsync<PageDto<MyOrderItemDto>>(await As(adherent).GetAsync($"{V1}/shop/orders", CancellationToken));
        Assert.Empty(mine.Items);
    }

    [Fact]
    public async Task Should_AllowANewOrder_When_TheFirstOneExpired()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var variantId = await GivenAnArticleAsync(board, quantity: 10);
        await GivenOrderAsync(adherent, (variantId, 1));
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 11, 0, 0, TimeSpan.Zero));

        var orderId = await GivenOrderAsync(adherent, (variantId, 2));

        Assert.Equal("pending", (await OrderAsync(adherent, orderId)).Status);
        Assert.Equal(2, (await VariantAsync(board, variantId)).Reserved);
    }
}
