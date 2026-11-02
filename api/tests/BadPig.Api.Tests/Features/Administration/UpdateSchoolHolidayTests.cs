using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class UpdateSchoolHolidayTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static string Url(string seasonId, string id) => $"{V1}/admin/seasons/{seasonId}/holidays/{id}";

    private async Task<List<HolidayDto>> HolidaysAsync(Person board, string seasonId) =>
        (await ReadAsync<PageDto<HolidayDto>>(await As(board).GetAsync($"{V1}/admin/seasons/{seasonId}/holidays", CancellationToken))).Items;

    private static object Body(string name = "Vacances de la Toussaint", string startsOn = "2026-10-17", string endsOn = "2026-11-02") =>
        new { name, startsOn, endsOn };

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync(Url(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()), Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).PutAsJsonAsync(Url(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()), Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_ThePeriodDoesNotExist()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);

        var response = await As(board).PutAsJsonAsync(Url(season, Guid.NewGuid().ToString()), Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_ThePeriodBelongsToAnotherSeason()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        var other = await GivenSeasonAsync(board, "2027-09-01");
        var id = await GivenHolidayAsync(board, season);

        var response = await As(board).PutAsJsonAsync(Url(other, id), Body(startsOn: "2027-10-17", endsOn: "2027-11-02"), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheDatesAreOutsideTheSeason()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        var id = await GivenHolidayAsync(board, season);

        var response = await As(board).PutAsJsonAsync(Url(season, id), Body(startsOn: "2026-08-20", endsOn: "2026-08-25"), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_TheDatesOverlapAnotherPeriod()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        await GivenHolidayAsync(board, season);
        var christmas = await GivenHolidayAsync(board, season, "Vacances de Noël", "2026-12-19", "2027-01-04");

        var response = await As(board).PutAsJsonAsync(Url(season, christmas), Body(name: "Vacances de Noël", startsOn: "2026-11-01", endsOn: "2027-01-04"), CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_ChangeThePeriod_When_BoardMemberChangesIt()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        var id = await GivenHolidayAsync(board, season);

        var response = await As(board).PutAsJsonAsync(Url(season, id), Body(name: " Toussaint ", startsOn: "2026-10-19", endsOn: "2026-11-01"), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var holiday = Assert.Single(await HolidaysAsync(board, season));
        Assert.Equal("Toussaint", holiday.Name);
        Assert.Equal("2026-10-19", holiday.StartsOn);
        Assert.Equal("2026-11-01", holiday.EndsOn);
    }

    [Fact]
    public async Task Should_AcceptItsOwnDates_When_OnlyTheNameChanges()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        var id = await GivenHolidayAsync(board, season);

        var response = await As(board).PutAsJsonAsync(Url(season, id), Body(name: "Toussaint"), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Should_RecordTheChangeInTheJournal()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        var id = await GivenHolidayAsync(board, season);

        await As(board).PutAsJsonAsync(Url(season, id), Body(name: "Toussaint"), CancellationToken);

        var journal = (await ReadAsync<PageDto<AuditEntryDto>>(await As(board).GetAsync($"{V1}/admin/audit-log", CancellationToken))).Items;
        Assert.Contains(journal, e => e.Action == "holiday.updated.Toussaint");
    }
}
