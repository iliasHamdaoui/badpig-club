using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class InterclubsAccountErasureTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_LeaveTheTeamWithoutCaptain_When_TheCaptainDeletesTheirAccount()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var teamId = await GivenTeamAsync(board, captain: captain);
        await GivenFixtureAsync(captain, teamId);

        await As(captain).DeleteAsync($"{V1}/me", CancellationToken);

        var team = Assert.Single(await TeamsAsync(board));
        Assert.Null(team.CaptainId);
        Assert.Null(team.CaptainName);
        Assert.Single(await FixturesAsync(board));
    }

    [Fact]
    public async Task Should_ForgetTheAnswersToTheProposedDates_When_TheAccountIsDeleted()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board, "Anna Adherente");
        var teamId = await GivenTeamAsync(board);
        var proposalId = await GivenProposalAsync(board, teamId);
        await GivenPlayerAsync(board, teamId, adherent);
        await GivenAnswerAsync(adherent, proposalId, 0, "yes");

        await As(adherent).DeleteAsync($"{V1}/me", CancellationToken);

        var date = (await ProposalAsync(board, proposalId)).Dates[0];
        Assert.Equal(0, date.Yes);
        Assert.Empty(date.Answers!);
    }

    [Fact]
    public async Task Should_RemoveThePlayerFromTheirTeams_When_TheAccountIsDeleted()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board, "Anna Adherente");
        var teamId = await GivenTeamAsync(board);
        await GivenPlayerAsync(board, teamId, adherent);

        await As(adherent).DeleteAsync($"{V1}/me", CancellationToken);

        Assert.Equal(0, Assert.Single(await TeamsAsync(board)).MemberCount);
    }

    [Fact]
    public async Task Should_RemoveThePlayerFromTheLineups_When_TheAccountIsDeleted()
    {
        var board = GivenBoardMember();
        var anna = await GivenAnAdherentAsync(board, "Anna Adherente");
        var teamId = await GivenTeamAsync(board);
        await GivenPlayerAsync(board, teamId, anna);
        var fixtureId = await GivenFixtureAsync(board, teamId);
        await As(board).PutAsJsonAsync($"{V1}/interclubs/fixtures/{fixtureId}/lineup", new { slots = new[] { new { slot = "SD", playerIds = new[] { anna.Id } } } }, CancellationToken);

        await As(anna).DeleteAsync($"{V1}/me", CancellationToken);

        Assert.All((await FixtureAsync(board, fixtureId)).Matches, m => Assert.Empty(m.Players));
    }
}
