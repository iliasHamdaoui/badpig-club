using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class ListFixturesTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string Url => $"{V1}/interclubs/fixtures";

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
    public async Task Should_ReturnBadRequest_When_SortPeriodTeamOrSeasonIsInvalid()
    {
        var board = GivenBoardMember();

        foreach (var query in new[] { "?sort=score", "?when=tomorrow", "?teamId=not-an-identifier", "?seasonId=not-an-identifier" })
            Assert.Equal(HttpStatusCode.BadRequest, (await As(board).GetAsync(Url + query, CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_ListTheCalendarByDate_When_AnAdherentAsks()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var d4 = await GivenTeamAsync(board, "D4");
        await GivenFixtureAsync(board, d4, "2026-12-05", "BC Nantes", "away");
        await GivenFixtureAsync(board, d4, "2026-11-14", "BC Lyon", "home", "14:00", "Gymnase Pignan");

        var fixtures = await FixturesAsync(adherent);

        Assert.Equal(["BC Lyon", "BC Nantes"], fixtures.Select(f => f.Opponent));
        var first = fixtures[0];
        Assert.Equal("D4", first.TeamName);
        Assert.Equal("2026-11-14", first.Date);
        Assert.Equal("14:00:00", first.StartTime);
        Assert.Equal("home", first.Venue);
        Assert.Equal("Gymnase Pignan", first.Place);
        Assert.Null(fixtures[1].StartTime);
    }

    [Fact]
    public async Task Should_SplitUpcomingAndPastFixtures_When_WhenIsGiven()
    {
        var board = GivenBoardMember();
        var d4 = await GivenTeamAsync(board);
        await GivenFixtureAsync(board, d4, "2026-10-14", "Hier");
        await GivenFixtureAsync(board, d4, "2026-10-15", "Aujourdhui");
        await GivenFixtureAsync(board, d4, "2026-10-20", "Bientot");
        await GivenFixtureAsync(board, d4, "2026-10-01", "Avant");

        var upcoming = await FixturesAsync(board, "?when=upcoming");
        var past = await FixturesAsync(board, "?when=past");

        Assert.Equal(["Aujourdhui", "Bientot"], upcoming.Select(f => f.Opponent));
        Assert.Equal(["Hier", "Avant"], past.Select(f => f.Opponent));
    }

    [Fact]
    public async Task Should_FilterOnTheTeam_When_TeamIdIsGiven()
    {
        var board = GivenBoardMember();
        var d4 = await GivenTeamAsync(board, "D4");
        var d5 = await GivenTeamAsync(board, "D5");
        await GivenFixtureAsync(board, d4, opponent: "BC Lyon");
        await GivenFixtureAsync(board, d5, opponent: "BC Nantes");

        var fixtures = await FixturesAsync(board, $"?teamId={d5}");

        Assert.Equal("BC Nantes", Assert.Single(fixtures).Opponent);
    }

    [Fact]
    public async Task Should_FilterOnTheSeason_When_SeasonIdIsGiven()
    {
        var board = GivenBoardMember();
        var past = await GivenSeasonAsync(board, "2025-09-01");
        var old = await GivenTeamAsync(board, "D4", past);
        var now = await GivenTeamAsync(board, "D5");
        await GivenFixtureAsync(board, old, opponent: "BC Lyon");
        await GivenFixtureAsync(board, now, opponent: "BC Nantes");

        var fixtures = await FixturesAsync(board, $"?seasonId={past}&when=");

        Assert.Equal("BC Lyon", Assert.Single(fixtures).Opponent);
    }

    [Fact]
    public async Task Should_SearchOnOpponentAndTeamNames()
    {
        var board = GivenBoardMember();
        var d4 = await GivenTeamAsync(board, "D4");
        var pr = await GivenTeamAsync(board, "PR");
        await GivenFixtureAsync(board, d4, opponent: "BC Lyon");
        await GivenFixtureAsync(board, pr, opponent: "BC Nantes");

        Assert.Equal("BC Lyon", Assert.Single(await FixturesAsync(board, "?search=lyon")).Opponent);
        Assert.Equal("BC Nantes", Assert.Single(await FixturesAsync(board, "?search=pr")).Opponent);
    }

    [Fact]
    public async Task Should_SortOnOpponent_When_AskedTo()
    {
        var board = GivenBoardMember();
        var d4 = await GivenTeamAsync(board);
        await GivenFixtureAsync(board, d4, "2026-11-01", "BC Nantes");
        await GivenFixtureAsync(board, d4, "2026-11-02", "BC Lyon");

        var fixtures = await FixturesAsync(board, "?sort=opponent");

        Assert.Equal(["BC Lyon", "BC Nantes"], fixtures.Select(f => f.Opponent));
    }

    [Fact]
    public async Task Should_ListOnlyTheFixtureBeingPlayed_When_TheLivePeriodIsAsked()
    {
        // The test clock says 15 October 2026 at noon in Paris.
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);
        var playing = await GivenFixtureAsync(board, teamId, "2026-10-15", "BC Direct", startTime: "11:00");
        await GivenFixtureAsync(board, teamId, "2026-10-15", "BC Ce Soir", startTime: "20:00");
        await GivenFixtureAsync(board, teamId, "2026-10-16", "BC Demain");
        await GivenFixtureAsync(board, teamId, "2026-10-10", "BC Hier");

        var live = await FixturesAsync(board, "?when=live");

        var item = Assert.Single(live);
        Assert.Equal((playing, "BC Direct", true), (item.Id, item.Opponent, item.IsLive));
    }

    [Fact]
    public async Task Should_CountAFixtureWithoutAStartTimeAsLive_When_ItIsToday()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);
        await GivenFixtureAsync(board, teamId, "2026-10-15", "BC Sans Heure");

        Assert.True(Assert.Single(await FixturesAsync(board, "?when=live")).IsLive);
    }

    [Fact]
    public async Task Should_StopBeingLive_When_TheSevenMatchesAreDecided()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);
        var fixtureId = await GivenFixtureAsync(board, teamId, "2026-10-15", "BC Fini", startTime: "11:00");
        await GivenMatchResultAsync(board, fixtureId, "SD");
        Assert.Single(await FixturesAsync(board, "?when=live"));

        await GivenResultAsync(board, fixtureId, "SD", "MX1", "MX2", "DD");

        Assert.Empty(await FixturesAsync(board, "?when=live"));
        var item = Assert.Single(await FixturesAsync(board, "?search=fini"));
        Assert.Equal((4, 3, false), (item.ClubScore, item.OpponentScore, item.IsLive));
    }

    [Fact]
    public async Task Should_FollowTheMatchesEnteredSoFar_When_TheResultIsBeingEntered()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);
        var fixtureId = await GivenFixtureAsync(board, teamId, "2026-10-15", "BC En Cours", startTime: "11:00");
        await GivenMatchResultAsync(board, fixtureId, "SD");
        await GivenMatchResultAsync(board, fixtureId, "MX1", won: false);

        var item = Assert.Single(await FixturesAsync(board, "?when=live"));

        Assert.Equal((2, 1, 1), (item.DecidedMatches, item.ClubMatches, item.OpponentMatches));
        Assert.Null(item.ClubScore);
    }
}
