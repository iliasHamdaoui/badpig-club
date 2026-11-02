using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class SetLineupTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string Url(string fixtureId) => $"{V1}/interclubs/fixtures/{fixtureId}/lineup";

    private static object Slot(string slot, params Person[] players) => new { slot, playerIds = players.Select(p => p.Id).ToArray() };

    /// <summary>A team with its captain and four players (the club does not record gender, so the lineup is only checked structurally), and a fixture.</summary>
    private async Task<(Person Board, Person Captain, Person[] Players, string TeamId, string FixtureId)> GivenATeamWithAFixtureAsync()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var players = new[]
        {
            await GivenAnAdherentAsync(board, "Anna Adherente"), await GivenAnAdherentAsync(board, "Bob Adherent"),
            await GivenAnAdherentAsync(board, "Chloe Adherente"), await GivenAnAdherentAsync(board, "Dan Adherent"),
        };
        var teamId = await GivenTeamAsync(board, captain: captain);
        foreach (var player in players)
            await GivenPlayerAsync(captain, teamId, player);

        return (board, captain, players, teamId, await GivenFixtureAsync(captain, teamId));
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync(Url(Guid.NewGuid().ToString()), new { slots = Array.Empty<object>() }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_FixtureDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PutAsJsonAsync(Url(Guid.NewGuid().ToString()), new { slots = Array.Empty<object>() }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotTheCaptainOfTheTeam()
    {
        var (_, _, players, _, fixtureId) = await GivenATeamWithAFixtureAsync();

        var response = await As(players[0]).PutAsJsonAsync(Url(fixtureId), new { slots = new[] { Slot("SD", players[0]) } }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheLineupIsStructurallyInvalid()
    {
        var (_, captain, p, _, fixtureId) = await GivenATeamWithAFixtureAsync();
        var url = Url(fixtureId);

        // Unknown slot, wrong number of players, same player twice in a double, same slot twice, invalid identifier, no body.
        Assert.Equal(HttpStatusCode.BadRequest, (await As(captain).PutAsJsonAsync(url, new { slots = new[] { Slot("XX", p[0]) } }, CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(captain).PutAsJsonAsync(url, new { slots = new[] { Slot("SD", p[0], p[1]) } }, CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(captain).PutAsJsonAsync(url, new { slots = new[] { Slot("DH", p[0]) } }, CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(captain).PutAsJsonAsync(url, new { slots = new[] { Slot("DH", p[0], p[0]) } }, CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(captain).PutAsJsonAsync(url, new { slots = new[] { Slot("SD", p[0]), Slot("SD", p[1]) } }, CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(captain).PutAsJsonAsync(url, new { slots = new[] { new { slot = "SD", playerIds = new[] { "nope" } } } }, CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(captain).PutAsJsonAsync(url, new { }, CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_APlayerIsNotInTheTeam()
    {
        var (board, captain, _, _, fixtureId) = await GivenATeamWithAFixtureAsync();
        var outsider = await GivenAnAdherentAsync(board, "Olga Autre");

        var response = await As(captain).PutAsJsonAsync(Url(fixtureId), new { slots = new[] { Slot("SD", outsider) } }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.All((await FixtureAsync(captain, fixtureId)).Matches, m => Assert.Empty(m.Players));
    }

    [Fact]
    public async Task Should_RecordTheLineup_When_TheCaptainChoosesThePlayers()
    {
        var (_, captain, p, _, fixtureId) = await GivenATeamWithAFixtureAsync();

        var response = await As(captain).PutAsJsonAsync(Url(fixtureId),
            new { slots = new[] { Slot("SD", p[0]), Slot("DH", p[1], p[3]), Slot("MX1", p[0], p[1]) } }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var matches = (await FixtureAsync(captain, fixtureId)).Matches;
        Assert.Equal(["SD", "MX1", "MX2", "DD", "DH", "SH1", "SH2"], matches.Select(m => m.Slot));
        Assert.Equal(["Anna Adherente"], matches[0].Players.Select(x => x.Name));
        Assert.Equal(["Anna Adherente", "Bob Adherent"], matches[1].Players.Select(x => x.Name));
        Assert.Empty(matches[2].Players);
        Assert.Equal(["Bob Adherent", "Dan Adherent"], matches[4].Players.Select(x => x.Name));
        Assert.Empty(matches[5].Players);
    }

    [Fact]
    public async Task Should_ReplaceThePreviousLineup_When_ItIsSentAgain()
    {
        var (_, captain, p, _, fixtureId) = await GivenATeamWithAFixtureAsync();
        await As(captain).PutAsJsonAsync(Url(fixtureId), new { slots = new[] { Slot("SD", p[0]), Slot("SH1", p[1]) } }, CancellationToken);

        await As(captain).PutAsJsonAsync(Url(fixtureId), new { slots = new[] { Slot("SD", p[2]) } }, CancellationToken);

        var matches = (await FixtureAsync(captain, fixtureId)).Matches;
        Assert.Equal("Chloe Adherente", Assert.Single(matches[0].Players).Name);
        Assert.Empty(matches[5].Players);
    }

    [Fact]
    public async Task Should_KeepTheOutcome_When_ASlotIsEmptied()
    {
        var board = GivenBoardMember();
        var player = await GivenAnAdherentAsync(board, "Anna Adherente");
        var teamId = await GivenTeamAsync(board);
        await GivenPlayerAsync(board, teamId, player);
        var fixtureId = await GivenFixtureAsync(board, teamId, "2026-10-10");
        await As(board).PutAsJsonAsync(Url(fixtureId), new { slots = new[] { Slot("SD", player) } }, CancellationToken);
        await GivenResultAsync(board, fixtureId, "SD", "MX1", "MX2", "DD");

        await As(board).PutAsJsonAsync(Url(fixtureId), new { slots = Array.Empty<object>() }, CancellationToken);

        var fixture = await FixtureAsync(board, fixtureId);
        Assert.Empty(fixture.Matches[0].Players);
        Assert.Equal("won", fixture.Matches[0].Outcome);
        Assert.Equal(4, fixture.ClubScore);
    }
}
