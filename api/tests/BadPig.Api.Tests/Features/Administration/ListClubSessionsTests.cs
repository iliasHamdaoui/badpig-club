using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class ListClubSessionsTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Url = $"{V1}/club/sessions";

    private async Task<ClubSessionsDto> SessionsAsync(string query = "") =>
        await ReadAsync<ClubSessionsDto>(await Anonymous().GetAsync(Url + query, CancellationToken));

    [Fact]
    public async Task Should_TellWhenTheClubPlays_When_NobodyIsSignedIn()
    {
        var sessions = await SessionsAsync(); // Thursday 15 October 2026: no session

        Assert.Equal("2026-10-15", sessions.Today);
        Assert.Equal(["tuesday", "wednesday", "friday"], sessions.Weekly.Select(d => d.Day));
        Assert.All(sessions.Weekly, d => Assert.Equal("20:00:00", d.StartTime));
        Assert.False(sessions.IsOpenToday);
        Assert.Null(sessions.Closure);
        Assert.Equal(["2026-10-16", "2026-10-20", "2026-10-21"], sessions.Next.Select(s => s.Date));
        Assert.All(sessions.Next, s => Assert.False(s.IsToday));
    }

    [Fact]
    public async Task Should_SayThereIsASessionToday_When_TodayIsAPlayingDay()
    {
        GivenTodayIs(2026, 10, 16); // a Friday

        var sessions = await SessionsAsync();

        Assert.True(sessions.IsOpenToday);
        Assert.Null(sessions.Closure);
        var today = sessions.Next[0];
        Assert.Equal("2026-10-16", today.Date);
        Assert.True(today.IsToday);
        Assert.Equal("23:00:00", today.EndTime);
    }

    [Fact]
    public async Task Should_SayTheClubIsClosed_When_TodayIsInTheSchoolHolidays()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        await GivenHolidayAsync(board, season); // 17 October to 2 November
        GivenTodayIs(2026, 10, 20); // a Tuesday, inside the holidays

        var sessions = await SessionsAsync();

        Assert.False(sessions.IsOpenToday);
        Assert.Equal("Vacances de la Toussaint", sessions.Closure!.Name);
        Assert.Equal("2026-11-02", sessions.Closure.EndsOn);
        Assert.Equal("2026-11-03", sessions.Next[0].Date);
    }

    [Fact]
    public async Task Should_SkipTheHolidays_When_TheyCutIntoTheNextSessions()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        await GivenHolidayAsync(board, season); // 17 October to 2 November
        GivenTodayIs(2026, 10, 16); // the Friday before the holidays

        var sessions = await SessionsAsync();

        Assert.True(sessions.IsOpenToday);
        Assert.Equal(["2026-10-16", "2026-11-03", "2026-11-04"], sessions.Next.Select(s => s.Date));
    }

    [Fact]
    public async Task Should_ReturnAsManySessionsAsAsked()
    {
        var sessions = await SessionsAsync("?count=5");

        Assert.Equal(5, sessions.Next.Count);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheCountIsOutOfRange()
    {
        foreach (var query in new[] { "?count=0", "?count=11" })
            Assert.Equal(HttpStatusCode.BadRequest, (await Anonymous().GetAsync(Url + query, CancellationToken)).StatusCode);
    }
}
