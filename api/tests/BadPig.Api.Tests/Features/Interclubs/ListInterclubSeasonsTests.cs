using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class ListInterclubSeasonsTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string Url => $"{V1}/interclubs/seasons";

    private async Task<List<InterclubSeasonDto>> SeasonsAsync(Person reader, string query = "") =>
        (await ReadAsync<PageDto<InterclubSeasonDto>>(await As(reader).GetAsync($"{Url}{query}", CancellationToken))).Items;

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync(Url, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_MembershipIsNotValidated()
    {
        var response = await As(GivenMember()).GetAsync(Url, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_SortIsUnknown()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{Url}?sort=teams", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ListTheSeasonsThatHaveTeams_When_SeveralSeasonsHaveBeenPlayed()
    {
        var board = GivenBoardMember();
        var past = await GivenSeasonAsync(board, "2025-09-01");
        var current = await GivenTheCurrentSeasonAsync(board);
        await GivenTeamAsync(board, "D4", past);
        await GivenTeamAsync(board, "D5", current);
        await GivenTeamAsync(board, "D6", current);

        var seasons = await SeasonsAsync(board);

        Assert.Equal(["2026-2027", "2025-2026"], seasons.Select(s => s.Name));
        Assert.Equal([2, 1], seasons.Select(s => s.TeamCount));
    }

    [Fact]
    public async Task Should_KeepAndMarkTheCurrentSeason_When_ItHasNoTeamYet()
    {
        var board = GivenBoardMember();
        var current = await GivenTheCurrentSeasonAsync(board);

        var season = Assert.Single(await SeasonsAsync(board));

        Assert.Equal(current, season.Id);
        Assert.True(season.IsCurrent);
        Assert.Equal(0, season.TeamCount);
    }

    [Fact]
    public async Task Should_LeaveOutASeasonWithoutTeam_When_ItIsNotTheCurrentOne()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board, "2025-09-01");
        await GivenTeamAsync(board, "D4");

        var seasons = await SeasonsAsync(board);

        Assert.Equal("2026-2027", Assert.Single(seasons).Name);
    }

    [Fact]
    public async Task Should_ListTheSeasons_When_AnAdherentAsks()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        await GivenTeamAsync(board, "D4");

        var seasons = await SeasonsAsync(adherent);

        Assert.Equal("2026-2027", Assert.Single(seasons).Name);
    }
}
