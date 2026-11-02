using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class DeleteSchoolHolidayTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static string Url(string seasonId, string id) => $"{V1}/admin/seasons/{seasonId}/holidays/{id}";

    private async Task<List<HolidayDto>> HolidaysAsync(Person board, string seasonId) =>
        (await ReadAsync<PageDto<HolidayDto>>(await As(board).GetAsync($"{V1}/admin/seasons/{seasonId}/holidays", CancellationToken))).Items;

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync(Url(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).DeleteAsync(Url(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_ThePeriodDoesNotExist()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);

        var response = await As(board).DeleteAsync(Url(season, Guid.NewGuid().ToString()), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_DeleteThePeriodAndBringTheSessionsBack_When_BoardMemberDeletesIt()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        var id = await GivenHolidayAsync(board, season);
        GivenTodayIs(2026, 10, 20); // a Tuesday inside the holidays

        var response = await As(board).DeleteAsync(Url(season, id), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await HolidaysAsync(board, season));
        var sessions = await ReadAsync<ClubSessionsDto>(await Anonymous().GetAsync($"{V1}/club/sessions", CancellationToken));
        Assert.True(sessions.IsOpenToday);
        Assert.Null(sessions.Closure);
    }

    [Fact]
    public async Task Should_RecordTheDeletionInTheJournal()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        var id = await GivenHolidayAsync(board, season);

        await As(board).DeleteAsync(Url(season, id), CancellationToken);

        var journal = (await ReadAsync<PageDto<AuditEntryDto>>(await As(board).GetAsync($"{V1}/admin/audit-log", CancellationToken))).Items;
        Assert.Contains(journal, e => e.Action == "holiday.deleted.Vacances de la Toussaint");
    }

    [Fact]
    public async Task Should_DeleteThePeriods_When_TheSeasonIsDeleted()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        await GivenHolidayAsync(board, season);
        GivenTodayIs(2026, 10, 20); // a Tuesday inside the holidays

        Assert.Equal(HttpStatusCode.NoContent, (await As(board).DeleteAsync($"{V1}/admin/seasons/{season}", CancellationToken)).StatusCode);

        var sessions = await ReadAsync<ClubSessionsDto>(await Anonymous().GetAsync($"{V1}/club/sessions", CancellationToken));
        Assert.Null(sessions.Closure);
    }
}
