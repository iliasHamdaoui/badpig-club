using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class ListTeamsTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string Url => $"{V1}/interclubs/teams";

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
        var response = await As(GivenBoardMember()).GetAsync($"{Url}?sort=captain", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ListTheTeamsWithTheirCaptain_When_AnAdherentAsks()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        await GivenTeamAsync(board, "D4", captain: captain);
        await GivenTeamAsync(board, "D5");

        var teams = await TeamsAsync(adherent);

        Assert.Equal(["D4", "D5"], teams.Select(t => t.Name));
        Assert.Equal("Carla Capitaine", teams[0].CaptainName);
        Assert.Null(teams[1].CaptainName);
    }

    [Fact]
    public async Task Should_TellWhichTeamIsMine_When_TheCaptainAsks()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        await GivenTeamAsync(board, "D4", captain: captain);
        await GivenTeamAsync(board, "D5");

        var teams = await TeamsAsync(captain);

        Assert.True(teams.Single(t => t.Name == "D4").IsMyTeam);
        Assert.False(teams.Single(t => t.Name == "D5").IsMyTeam);
    }

    [Fact]
    public async Task Should_ShowTheLatestSeasonFirst_When_NoSortIsGiven()
    {
        var board = GivenBoardMember();
        var past = await GivenSeasonAsync(board, "2025-09-01");
        var current = await GivenTheCurrentSeasonAsync(board);
        await GivenTeamAsync(board, "D4", past);
        await GivenTeamAsync(board, "D6", current);
        await GivenTeamAsync(board, "D5", current);

        var teams = await TeamsAsync(board);

        Assert.Equal(["D5", "D6", "D4"], teams.Select(t => t.Name));
    }

    [Fact]
    public async Task Should_FilterOnTheSeason_When_SeasonIsGiven()
    {
        var board = GivenBoardMember();
        var past = await GivenSeasonAsync(board, "2025-09-01");
        await GivenTeamAsync(board, "D4", past);
        await GivenTeamAsync(board, "D5");

        var teams = await TeamsAsync(board, $"?seasonId={past}");

        var team = Assert.Single(teams);
        Assert.Equal("D4", team.Name);
        Assert.Equal("2025-2026", team.Season);
    }

    [Fact]
    public async Task Should_SearchOnTeamAndCaptainNames()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        await GivenTeamAsync(board, "D4", captain: captain);
        await GivenTeamAsync(board, "PR");

        Assert.Equal("D4", Assert.Single(await TeamsAsync(board, "?search=carla")).Name);
        Assert.Equal("PR", Assert.Single(await TeamsAsync(board, "?search=pr")).Name);
    }

    [Fact]
    public async Task Should_SortOnNameDescending_When_AskedTo()
    {
        var board = GivenBoardMember();
        await GivenTeamAsync(board, "D4");
        await GivenTeamAsync(board, "D5");

        var teams = await TeamsAsync(board, "?sort=name&direction=Desc");

        Assert.Equal(["D5", "D4"], teams.Select(t => t.Name));
    }

    [Fact]
    public async Task Should_GiveThePlayerCountAndTellWhichTeamsIPlayIn()
    {
        var board = GivenBoardMember();
        var anna = await GivenAnAdherentAsync(board, "Anna Adherente");
        var bob = await GivenAnAdherentAsync(board, "Bob Adherent");
        var d4 = await GivenTeamAsync(board, "D4");
        var d5 = await GivenTeamAsync(board, "D5");
        await GivenPlayerAsync(board, d4, anna);
        await GivenPlayerAsync(board, d4, bob);

        var teams = await TeamsAsync(anna);

        var team4 = teams.Single(t => t.Id == d4);
        Assert.Equal(2, team4.MemberCount);
        Assert.True(team4.IsPlayer);
        var team5 = teams.Single(t => t.Id == d5);
        Assert.Equal(0, team5.MemberCount);
        Assert.False(team5.IsPlayer);
    }
}
