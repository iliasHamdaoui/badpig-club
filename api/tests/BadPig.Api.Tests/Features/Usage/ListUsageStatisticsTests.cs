using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Usage;

public sealed class ListUsageStatisticsTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Url = $"{V1}/admin/usage";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync(Url, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).GetAsync(Url, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnAnEmptyList_When_NothingWasRecorded()
    {
        Assert.Empty(await UsageAsync(GivenBoardMember()));
    }

    [Fact]
    public async Task Should_ListTheMostOpenedPageFirst_When_NoSortIsGiven()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        await GivenPageVisitAsync(member, "/club", 5);
        await GivenPageVisitAsync(member, "/shop", 5);
        await GivenPageVisitAsync(member, "/shop", 5);

        var items = await UsageAsync(board);

        Assert.Equal(["/shop", "/club"], items.Select(i => i.Path).ToArray());
    }

    [Fact]
    public async Task Should_SortByTheTimeSpent_When_IAskForSeconds()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        await GivenPageVisitAsync(member, "/club", 300);
        await GivenPageVisitAsync(member, "/shop", 5);
        await GivenPageVisitAsync(member, "/shop", 5);

        var items = await UsageAsync(board, "?sort=seconds");

        Assert.Equal(["/club", "/shop"], items.Select(i => i.Path).ToArray());
    }

    [Fact]
    public async Task Should_FilterOnThePage_When_ISearch()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        await GivenPageVisitAsync(member, "/club");
        await GivenPageVisitAsync(member, "/shop/orders");

        var items = await UsageAsync(board, "?search=orders");

        Assert.Equal("/shop/orders", Assert.Single(items).Path);
    }

    [Fact]
    public async Task Should_LeaveOutTheOldVisits_When_ThePeriodIsShorter()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        await GivenPageVisitAsync(member, "/club");
        GivenTodayIs(2026, 11, 25);
        await GivenPageVisitAsync(member, "/shop");

        // The first visit is 41 days old.
        var lastMonth = await UsageAsync(board, "?days=30");
        var lastQuarter = await UsageAsync(board, "?days=90");

        Assert.Equal("/shop", Assert.Single(lastMonth).Path);
        Assert.Equal(2, lastQuarter.Count);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_ThePeriodOrTheSortIsUnknown()
    {
        var board = GivenBoardMember();

        var period = await As(board).GetAsync($"{Url}?days=10", CancellationToken);
        var sort = await As(board).GetAsync($"{Url}?sort=inconnu", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, period.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, sort.StatusCode);
    }
}
