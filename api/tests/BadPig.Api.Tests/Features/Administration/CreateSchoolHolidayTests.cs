using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class CreateSchoolHolidayTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static string Url(string seasonId) => $"{V1}/admin/seasons/{seasonId}/holidays";

    private async Task<List<HolidayDto>> HolidaysAsync(Person board, string seasonId) =>
        (await ReadAsync<PageDto<HolidayDto>>(await As(board).GetAsync(Url(seasonId), CancellationToken))).Items;

    private async Task<List<AuditEntryDto>> AuditLogAsync(Person board) =>
        (await ReadAsync<PageDto<AuditEntryDto>>(await As(board).GetAsync($"{V1}/admin/audit-log", CancellationToken))).Items;

    private static object Body(string name = "Vacances de la Toussaint", string startsOn = "2026-10-17", string endsOn = "2026-11-02") =>
        new { name, startsOn, endsOn };

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync(Url(Guid.NewGuid().ToString()), Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).PostAsJsonAsync(Url(Guid.NewGuid().ToString()), Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TheSeasonDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Url(Guid.NewGuid().ToString()), Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_NameOrDatesAreMissingOrWrong()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);

        var noName = await As(board).PostAsJsonAsync(Url(season), Body(name: " "), CancellationToken);
        var noDates = await As(board).PostAsJsonAsync(Url(season), new { name = "Noël" }, CancellationToken);
        var backwards = await As(board).PostAsJsonAsync(Url(season), Body(startsOn: "2026-11-02", endsOn: "2026-10-17"), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, noName.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, noDates.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, backwards.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheDatesAreOutsideTheSeason()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);

        var response = await As(board).PostAsJsonAsync(Url(season), Body(startsOn: "2026-08-20", endsOn: "2026-08-25"), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_TheDatesOverlapAnotherPeriod()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        await GivenHolidayAsync(board, season);

        var response = await As(board).PostAsJsonAsync(Url(season), Body(name: "Autre", startsOn: "2026-11-02", endsOn: "2026-11-08"), CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_AddThePeriod_When_BoardMemberCreatesIt()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);

        var response = await As(board).PostAsJsonAsync(Url(season), Body(name: " Vacances de la Toussaint "), CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var holiday = Assert.Single(await HolidaysAsync(board, season));
        Assert.Equal((await ReadAsync<IdDto>(response)).Id, holiday.Id);
        Assert.Equal("Vacances de la Toussaint", holiday.Name);
        Assert.Equal("2026-10-17", holiday.StartsOn);
        Assert.Equal("2026-11-02", holiday.EndsOn);
    }

    [Fact]
    public async Task Should_RecordTheCreationInTheJournal()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);

        await GivenHolidayAsync(board, season);

        Assert.Contains(await AuditLogAsync(board), e => e.Action == "holiday.created.Vacances de la Toussaint");
    }
}
