using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class SetFixtureResultTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string Url(string fixtureId) => $"{V1}/interclubs/fixtures/{fixtureId}/result";

    private static object[] TwoNil(int club, int opponent) => [new { club, opponent }, new { club, opponent }];

    /// <summary>A body where every match has the given sets.</summary>
    private static object SameSetsEverywhere(params (int Club, int Opponent)[] sets) =>
        new { matches = MatchSlotCodes.Select(slot => new { slot, sets = sets.Select(s => new { club = s.Club, opponent = s.Opponent }).ToArray() }).ToArray() };

    private async Task<string> GivenAPlayedFixtureAsync(Person board, string date = "2026-10-10") =>
        await GivenFixtureAsync(board, await GivenTeamAsync(board), date);

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync(Url(Guid.NewGuid().ToString()), ResultBody(), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_FixtureDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PutAsJsonAsync(Url(Guid.NewGuid().ToString()), ResultBody(), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotTheCaptainOfTheTeam()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var fixtureId = await GivenAPlayedFixtureAsync(board);

        var response = await As(adherent).PutAsJsonAsync(Url(fixtureId), ResultBody("SD"), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Null((await FixtureAsync(board, fixtureId)).ClubScore);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_TheFixtureHasNotBeenPlayedYet()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board, "2026-10-16");

        var response = await As(board).PutAsJsonAsync(Url(fixtureId), ResultBody("SD"), CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_AcceptTheResult_When_TheFixtureIsToday()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board, "2026-10-15");

        var response = await As(board).PutAsJsonAsync(Url(fixtureId), ResultBody("SD"), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_AMatchIsMissingOrRepeated()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);
        var all = MatchSlotCodes.Select(slot => new { slot, sets = TwoNil(15, 5) }).ToList();
        var six = new { matches = all.Take(6).ToArray() };
        var twice = new { matches = all.Take(6).Append(all[0]).ToArray() };

        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(Url(fixtureId), six, CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(Url(fixtureId), twice, CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(Url(fixtureId), new { }, CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_ASetIsNotValid()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);

        // Nobody reached 15; a draw; 15-14 (no lead of 2); 16-13 (a lead of 3 after 15); 16-15; 22-20 (past the cap of 21); 21-18; negative points; a set without points.
        (int, int)[] invalid = [(14, 10), (15, 15), (14, 14), (15, 14), (16, 13), (16, 15), (22, 20), (21, 18), (21, 21), (15, -1)];
        foreach (var (club, opponent) in invalid)
            Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(Url(fixtureId), SameSetsEverywhere((club, opponent), (15, 10)), CancellationToken)).StatusCode);

        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(Url(fixtureId), new { matches = MatchSlotCodes.Select(slot => new { slot, sets = new[] { new { club = (int?)15, opponent = (int?)null } } }).ToArray() }, CancellationToken)).StatusCode);
        Assert.Null((await FixtureAsync(board, fixtureId)).ClubScore);
    }

    [Fact]
    public async Task Should_AcceptTheSet_When_ItIsWonAtFifteenOnALeadOfTwoOrAtTwentyOne()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);

        // 15-0, 15-13, 16-14, 17-15, 20-18, 21-19, 21-20 and the same for the opponent.
        (int, int)[] valid = [(15, 0), (15, 13), (16, 14), (17, 15), (20, 18), (21, 19), (21, 20)];
        foreach (var (club, opponent) in valid)
        {
            Assert.Equal(HttpStatusCode.NoContent, (await As(board).PutAsJsonAsync(Url(fixtureId), SameSetsEverywhere((club, opponent), (club, opponent)), CancellationToken)).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await As(board).PutAsJsonAsync(Url(fixtureId), SameSetsEverywhere((opponent, club), (opponent, club)), CancellationToken)).StatusCode);
        }

        var read = await FixtureAsync(board, fixtureId);
        Assert.Equal((0, 7), (read.ClubScore, read.OpponentScore));
    }

    [Fact]
    public async Task Should_DecideTheMatchOnTheHigherScore_When_ASetGoesToDeuce()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);

        await As(board).PutAsJsonAsync(Url(fixtureId), SameSetsEverywhere((14, 16), (17, 15), (21, 20)), CancellationToken);

        var read = await FixtureAsync(board, fixtureId);
        Assert.All(read.Matches, m => Assert.Equal("won", m.Outcome));
        Assert.Equal(new SetDto(14, 16), read.Matches[0].Sets[0]);
    }

    private static object Entry(string slot, string? ending, string? withdrawn, params (int Club, int Opponent)[] sets) =>
        new { slot, ending, withdrawn, sets = sets.Select(s => new { club = s.Club, opponent = s.Opponent }).ToArray() };

    /// <summary>A body where the first match is the given entry and the six others are won by the club.</summary>
    private static object FirstMatchThen(object first) =>
        new { matches = new[] { first }.Concat(MatchSlotCodes.Skip(1).Select(slot => Entry(slot, null, null, (15, 5), (15, 5)))).ToArray() };

    [Fact]
    public async Task Should_AwardTheMatchToTheOtherSide_When_AMatchIsForfeited()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);

        var byClub = await As(board).PutAsJsonAsync(Url(fixtureId), FirstMatchThen(Entry("SD", "forfeit", "club")), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, byClub.StatusCode);
        var read = await FixtureAsync(board, fixtureId);
        var sd = read.Matches[0];
        Assert.Equal(("forfeit", "club", "lost"), (sd.Ending, sd.Withdrawn, sd.Outcome));
        Assert.Empty(sd.Sets);
        Assert.Equal((6, 1), (read.ClubScore, read.OpponentScore));

        await As(board).PutAsJsonAsync(Url(fixtureId), FirstMatchThen(Entry("SD", "forfeit", "opponent")), CancellationToken);

        read = await FixtureAsync(board, fixtureId);
        Assert.Equal(("forfeit", "opponent", "won"), (read.Matches[0].Ending, read.Matches[0].Withdrawn, read.Matches[0].Outcome));
        Assert.Equal((7, 0), (read.ClubScore, read.OpponentScore));
    }

    [Fact]
    public async Task Should_AwardTheMatchToTheOtherSideAndKeepTheSets_When_APlayerRetires()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);

        // The club was leading 1-0 and 10-4 in the second set, then its player retired: the match is lost anyway.
        var response = await As(board).PutAsJsonAsync(Url(fixtureId), FirstMatchThen(Entry("SD", "retirement", "club", (15, 9))), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var read = await FixtureAsync(board, fixtureId);
        var sd = read.Matches[0];
        Assert.Equal(("retirement", "club", "lost"), (sd.Ending, sd.Withdrawn, sd.Outcome));
        Assert.Equal(new SetDto(15, 9), Assert.Single(sd.Sets));
        Assert.Equal((6, 1), (read.ClubScore, read.OpponentScore));
    }

    [Fact]
    public async Task Should_AcceptARetirementWithoutAnySet_When_ThePlayerRetiresBeforeTheEndOfTheFirstSet()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);

        var response = await As(board).PutAsJsonAsync(Url(fixtureId), FirstMatchThen(Entry("SD", "retirement", "opponent")), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("won", (await FixtureAsync(board, fixtureId)).Matches[0].Outcome);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_AForfeitOrARetirementIsNotConsistent()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);
        object[] invalid =
        [
            Entry("SD", "forfeit", null),                                 // no side
            Entry("SD", "forfeit", "nobody"),                             // unknown side
            Entry("SD", "forfeit", "club", (15, 5)),                      // a forfeit has no sets
            Entry("SD", "retirement", null, (15, 5)),                     // no side
            Entry("SD", "retirement", "club", (15, 5), (15, 5)),          // the match was already decided
            Entry("SD", "retirement", "club", (15, 14)),                  // invalid set
            Entry("SD", "played", "club", (15, 5), (15, 5)),              // a played match has no side
            Entry("SD", "abandoned", "club"),                             // unknown ending
            Entry("SD", null, "club", (15, 5), (15, 5)),                  // played by default: no side either
        ];

        foreach (var entry in invalid)
            Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(Url(fixtureId), FirstMatchThen(entry), CancellationToken)).StatusCode);

        Assert.Null((await FixtureAsync(board, fixtureId)).ClubScore);
    }

    [Fact]
    public async Task Should_ShowAPlayedEnding_When_AMatchIsPlayedToTheEnd()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);

        await GivenResultAsync(board, fixtureId, "SD");

        var sd = (await FixtureAsync(board, fixtureId)).Matches[0];
        Assert.Equal(("played", null), (sd.Ending, sd.Withdrawn));
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_AMatchIsNotFinishedOrHasTooManySets()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);

        // One set only; 1-1 without a third set; a third set after a 2-0; four sets.
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(Url(fixtureId), SameSetsEverywhere((15, 10)), CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(Url(fixtureId), SameSetsEverywhere((15, 10), (9, 15)), CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(Url(fixtureId), SameSetsEverywhere((15, 10), (15, 8), (15, 5)), CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(Url(fixtureId), SameSetsEverywhere((15, 10), (9, 15), (15, 5), (15, 5)), CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_ComputeEachOutcomeFromTheSets_When_TheCaptainEntersTheResult()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var teamId = await GivenTeamAsync(board, captain: captain);
        var fixtureId = await GivenFixtureAsync(captain, teamId, "2026-10-10");
        var body = new
        {
            matches = new object[]
            {
                new { slot = "SD", sets = new[] { new { club = 15, opponent = 7 }, new { club = 15, opponent = 13 } } },          // won 2-0
                new { slot = "MX1", sets = new[] { new { club = 15, opponent = 12 }, new { club = 8, opponent = 15 }, new { club = 15, opponent = 13 } } }, // won 2-1
                new { slot = "MX2", sets = new[] { new { club = 10, opponent = 15 }, new { club = 15, opponent = 11 }, new { club = 9, opponent = 15 } } }, // lost 1-2
                new { slot = "DD", sets = new[] { new { club = 3, opponent = 15 }, new { club = 13, opponent = 15 } } },           // lost 0-2
                new { slot = "DH", sets = new[] { new { club = 15, opponent = 0 }, new { club = 15, opponent = 1 } } },           // won 2-0
                new { slot = "SH1", sets = new[] { new { club = 15, opponent = 13 }, new { club = 13, opponent = 15 }, new { club = 15, opponent = 13 } } }, // won 2-1
                new { slot = "SH2", sets = new[] { new { club = 5, opponent = 15 }, new { club = 5, opponent = 15 } } },           // lost 0-2
            },
        };

        var response = await As(captain).PutAsJsonAsync(Url(fixtureId), body, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var read = await FixtureAsync(captain, fixtureId);
        Assert.Equal(["won", "won", "lost", "lost", "won", "won", "lost"], read.Matches.Select(m => m.Outcome));
        Assert.Equal([2, 3, 3, 2, 2, 3, 2], read.Matches.Select(m => m.Sets.Count));
        Assert.Equal(new SetDto(8, 15), read.Matches[1].Sets[1]);
        Assert.Equal((4, 3), (read.ClubScore, read.OpponentScore));
        var item = Assert.Single(await FixturesAsync(captain));
        Assert.Equal((4, 3), (item.ClubScore, item.OpponentScore));
    }

    [Fact]
    public async Task Should_ScoreSevenToNothing_When_TheClubWinsEveryMatch()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);

        await GivenResultAsync(board, fixtureId, "SD", "MX1", "MX2", "DD", "DH", "SH1", "SH2");

        var read = await FixtureAsync(board, fixtureId);
        Assert.Equal((7, 0), (read.ClubScore, read.OpponentScore));
    }

    [Fact]
    public async Task Should_ReplaceThePreviousResult_When_ItIsEnteredAgain()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);
        await GivenResultAsync(board, fixtureId, "SD", "MX1", "DD", "SH2");

        await GivenResultAsync(board, fixtureId, "SD", "MX1", "MX2", "DD", "DH", "SH1");

        var read = await FixtureAsync(board, fixtureId);
        Assert.Equal((6, 1), (read.ClubScore, read.OpponentScore));
        Assert.All(read.Matches, m => Assert.Equal(2, m.Sets.Count));
    }

    [Fact]
    public async Task Should_KeepTheLineup_When_TheResultIsEntered()
    {
        var board = GivenBoardMember();
        var player = await GivenAnAdherentAsync(board, "Anna Adherente");
        var teamId = await GivenTeamAsync(board);
        await GivenPlayerAsync(board, teamId, player);
        var fixtureId = await GivenFixtureAsync(board, teamId, "2026-10-10");
        await As(board).PutAsJsonAsync($"{V1}/interclubs/fixtures/{fixtureId}/lineup", new { slots = new[] { new { slot = "SD", playerIds = new[] { player.Id } } } }, CancellationToken);

        await GivenResultAsync(board, fixtureId, "SD");

        Assert.Equal("Anna Adherente", Assert.Single((await FixtureAsync(board, fixtureId)).Matches[0].Players).Name);
    }

    [Fact]
    public async Task Should_ShowNoScoreAndNoSets_When_NoResultIsEntered()
    {
        var board = GivenBoardMember();
        var fixtureId = await GivenAPlayedFixtureAsync(board);

        var read = await FixtureAsync(board, fixtureId);

        Assert.Null(read.ClubScore);
        Assert.Null(read.OpponentScore);
        Assert.All(read.Matches, m => { Assert.Null(m.Outcome); Assert.Empty(m.Sets); });
        Assert.Null(Assert.Single(await FixturesAsync(board)).ClubScore);
    }
}
