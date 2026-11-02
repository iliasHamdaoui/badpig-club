using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class ListSchoolHolidaysTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static string Url(string seasonId) => $"{V1}/admin/seasons/{seasonId}/holidays";

    private async Task<List<HolidayDto>> HolidaysAsync(Person board, string seasonId, string query = "") =>
        (await ReadAsync<PageDto<HolidayDto>>(await As(board).GetAsync(Url(seasonId) + query, CancellationToken))).Items;

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync(Url(Guid.NewGuid().ToString()), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).GetAsync(Url(Guid.NewGuid().ToString()), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TheSeasonDoesNotExist()
    {
        var response = await As(GivenBoardMember()).GetAsync(Url(Guid.NewGuid().ToString()), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_SortIsUnknown()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);

        var response = await As(board).GetAsync($"{Url(season)}?sort=season", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ListThePeriodsOfTheSeason_EarliestFirst()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        await GivenHolidayAsync(board, season, "Vacances de Noël", "2026-12-19", "2027-01-04");
        await GivenHolidayAsync(board, season);

        var holidays = await HolidaysAsync(board, season);

        Assert.Equal(["Vacances de la Toussaint", "Vacances de Noël"], holidays.Select(h => h.Name));
    }

    [Fact]
    public async Task Should_LeaveOutThePeriodsOfAnotherSeason()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        var next = await GivenSeasonAsync(board, "2027-09-01");
        await GivenHolidayAsync(board, season);
        await GivenHolidayAsync(board, next, "Vacances de la Toussaint 2027", "2027-10-16", "2027-11-01");

        var holidays = await HolidaysAsync(board, next);

        Assert.Equal("Vacances de la Toussaint 2027", Assert.Single(holidays).Name);
    }

    [Fact]
    public async Task Should_SearchOnTheName()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        await GivenHolidayAsync(board, season);
        await GivenHolidayAsync(board, season, "Vacances de Noël", "2026-12-19", "2027-01-04");

        var holidays = await HolidaysAsync(board, season, "?search=Noël");

        Assert.Equal("Vacances de Noël", Assert.Single(holidays).Name);
    }

    [Fact]
    public async Task Should_SortOnTheNameDescending_When_AskedTo()
    {
        var board = GivenBoardMember();
        var season = await GivenSeasonAsync(board);
        await GivenHolidayAsync(board, season);
        await GivenHolidayAsync(board, season, "Vacances de Noël", "2026-12-19", "2027-01-04");

        var holidays = await HolidaysAsync(board, season, "?sort=name&direction=Desc");

        Assert.Equal(["Vacances de Noël", "Vacances de la Toussaint"], holidays.Select(h => h.Name));
    }
}
