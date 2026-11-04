using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Usage;

public sealed class GetUsageTrendTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Url = $"{V1}/admin/usage/trend";

    private async Task<List<UsageTrendDayDto>> TrendAsync(Person board, string query = "") =>
        (await ReadAsync<UsageTrendDto>(await As(board).GetAsync(Url + query, CancellationToken))).Days;

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
    public async Task Should_ReturnOneEmptyEntryPerDay_When_NothingWasRecorded()
    {
        var days = await TrendAsync(GivenBoardMember(), "?days=7");

        Assert.Equal(7, days.Count);
        Assert.Equal("2026-10-15", days[^1].Date);
        Assert.All(days, d => Assert.Equal((0, 0L), (d.Views, d.Seconds)));
    }

    [Fact]
    public async Task Should_AddUpAllThePagesOfTheDay_When_SeveralPagesWereOpened()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        await GivenPageVisitAsync(member, "/club", 30);
        await GivenPageVisitAsync(member, "/shop", 20);
        await GivenPageVisitAsync(member, "/shop", 10, views: 0);

        var today = (await TrendAsync(board))[^1];

        Assert.Equal(2, today.Views);
        Assert.Equal(60, today.Seconds);
    }

    [Fact]
    public async Task Should_KeepTheDaysApart_When_VisitsAreSpreadOverSeveralDays()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        await GivenPageVisitAsync(member, "/club", 30);
        GivenTodayIs(2026, 10, 17);
        await GivenPageVisitAsync(member, "/club", 10);

        var days = await TrendAsync(board, "?days=7");

        Assert.Equal(1, days.Single(d => d.Date == "2026-10-15").Views);
        Assert.Equal(0, days.Single(d => d.Date == "2026-10-16").Views);
        Assert.Equal(1, days.Single(d => d.Date == "2026-10-17").Views);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_ThePeriodIsUnknown()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{Url}?days=10", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
