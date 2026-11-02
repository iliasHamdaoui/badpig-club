using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class SetMatchResultTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string Url(string fixtureId, string slot = "SD") => $"{V1}/interclubs/fixtures/{fixtureId}/matches/{slot}/result";

    private static object TwoNil => new { sets = new[] { new { club = 15, opponent = 10 }, new { club = 15, opponent = 8 } } };

    private async Task<string> GivenAPlayedFixtureAsync(Person board, string date = "2026-10-10") =>
        await GivenFixtureAsync(board, await GivenTeamAsync(board), date);

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync(Url(Guid.NewGuid().ToString()), TwoNil, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_FixtureDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PutAsJsonAsync(Url(Guid.NewGuid().ToString()), TwoNil, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotTheCaptainOfTheTeam()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var fixtureId = await GivenAPlayedFixtureAsync(board);

        var response = await As(adherent).PutAsJsonAsync(Url(fixtureId), TwoNil, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, (await FixtureAsync(board, fixtureId)).Matches.Count(m => m.Outcome is not null));
    }

    [Fact]
    public async Task Should_ReturnConflict_When_TheFixtureHasNotBeenPlayedYet()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board, "2026-10-16");

        var response = await As(board).PutAsJsonAsync(Url(fixtureId), TwoNil, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheMatchIsUnknown()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);

        var response = await As(board).PutAsJsonAsync(Url(fixtureId, "SH3"), TwoNil, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheResultIsNotAFinishedMatch()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);
        object[] invalid =
        [
            new { sets = new[] { new { club = 15, opponent = 10 } } },                                        // one set only
            new { sets = new[] { new { club = 15, opponent = 14 }, new { club = 15, opponent = 8 } } },        // 15-14 is not a finished set
            new { ending = "forfeit", sets = Array.Empty<object>() },                                         // no side given
            new { ending = "played", withdrawn = "club", sets = new[] { new { club = 15, opponent = 10 }, new { club = 15, opponent = 8 } } },
        ];

        foreach (var body in invalid)
            Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(Url(fixtureId), body, CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_RecordOnlyThatMatch_When_TheCaptainEntersItAlone()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var teamId = await GivenTeamAsync(board, captain: captain);
        var fixtureId = await GivenFixtureAsync(captain, teamId, "2026-10-10");

        var response = await As(captain).PutAsJsonAsync(Url(fixtureId, "MX1"), TwoNil, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var read = await FixtureAsync(captain, fixtureId);
        var mixed = Assert.Single(read.Matches, m => m.Slot == "MX1");
        Assert.Equal(("played", "won"), (mixed.Ending, mixed.Outcome));
        Assert.Equal(2, mixed.Sets.Count);
        Assert.All(read.Matches.Where(m => m.Slot != "MX1"), m => Assert.Null(m.Outcome));
        // The score of the fixture stays unknown until the seven matches are decided.
        Assert.Null(read.ClubScore);
    }

    [Fact]
    public async Task Should_GiveTheScoreOfTheFixture_When_TheSevenMatchesHaveBeenEnteredOneByOne()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);

        foreach (var slot in MatchSlotCodes)
            await GivenMatchResultAsync(board, fixtureId, slot, won: slot != "SH2");

        var read = await FixtureAsync(board, fixtureId);
        Assert.Equal((6, 1), (read.ClubScore, read.OpponentScore));
        var item = Assert.Single(await FixturesAsync(board));
        Assert.Equal((7, 6, 1), (item.DecidedMatches, item.ClubMatches, item.OpponentMatches));
    }

    [Fact]
    public async Task Should_ReplaceOnlyThatMatch_When_ItIsEnteredAgain()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);
        await GivenMatchResultAsync(board, fixtureId, "SD");
        await GivenMatchResultAsync(board, fixtureId, "DD");

        await GivenMatchResultAsync(board, fixtureId, "SD", won: false);

        var read = await FixtureAsync(board, fixtureId);
        Assert.Equal("lost", Assert.Single(read.Matches, m => m.Slot == "SD").Outcome);
        Assert.Equal("won", Assert.Single(read.Matches, m => m.Slot == "DD").Outcome);
        var item = Assert.Single(await FixturesAsync(board));
        Assert.Equal((2, 1, 1), (item.DecidedMatches, item.ClubMatches, item.OpponentMatches));
    }

    [Fact]
    public async Task Should_AwardTheMatchToTheOtherSide_When_OneMatchIsForfeited()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);

        var response = await As(board).PutAsJsonAsync(Url(fixtureId, "DH"), new { ending = "forfeit", withdrawn = "opponent" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var match = Assert.Single((await FixtureAsync(board, fixtureId)).Matches, m => m.Slot == "DH");
        Assert.Equal(("forfeit", "opponent", "won"), (match.Ending, match.Withdrawn, match.Outcome));
        Assert.Empty(match.Sets);
    }
}
