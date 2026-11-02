using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class RemoveTeamMemberTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string Url(string teamId, string accountId) => $"{V1}/interclubs/teams/{teamId}/members/{accountId}";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync(Url(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TeamDoesNotExistOrPersonIsNotAPlayer()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var teamId = await GivenTeamAsync(board);

        Assert.Equal(HttpStatusCode.NotFound, (await As(board).DeleteAsync(Url(Guid.NewGuid().ToString(), adherent.Id), CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await As(board).DeleteAsync(Url(teamId, adherent.Id), CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotTheCaptainOfTheTeam()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var teamId = await GivenTeamAsync(board);
        await GivenPlayerAsync(board, teamId, adherent);

        var response = await As(adherent).DeleteAsync(Url(teamId, adherent.Id), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(1, Assert.Single(await TeamsAsync(board)).MemberCount);
    }

    [Fact]
    public async Task Should_RemoveThePlayer_When_TheCaptainRemovesThem()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var anna = await GivenAnAdherentAsync(board, "Anna Adherente");
        var teamId = await GivenTeamAsync(board, captain: captain);
        await GivenPlayerAsync(captain, teamId, anna);

        var response = await As(captain).DeleteAsync(Url(teamId, anna.Id), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, Assert.Single(await TeamsAsync(captain)).MemberCount);
    }

    [Fact]
    public async Task Should_WithdrawTheirAnswersToOpenProposals_When_ThePlayerIsRemoved()
    {
        var board = GivenBoardMember();
        var anna = await GivenAnAdherentAsync(board, "Anna Adherente");
        var teamId = await GivenTeamAsync(board);
        var proposalId = await GivenProposalAsync(board, teamId);
        await GivenPlayerAsync(board, teamId, anna);
        await GivenAnswerAsync(anna, proposalId, 0, "yes");

        await As(board).DeleteAsync(Url(teamId, anna.Id), CancellationToken);

        var date = (await ProposalAsync(board, proposalId)).Dates[0];
        Assert.Equal(0, date.Yes);
        Assert.Empty(date.Answers!);
    }

    [Fact]
    public async Task Should_KeepTheirAnswersToClosedProposals_When_ThePlayerIsRemoved()
    {
        var board = GivenBoardMember();
        var anna = await GivenAnAdherentAsync(board, "Anna Adherente");
        var teamId = await GivenTeamAsync(board);
        var proposalId = await GivenProposalAsync(board, teamId);
        await GivenPlayerAsync(board, teamId, anna);
        await GivenAnswerAsync(anna, proposalId, 0, "yes");
        var dates = (await ProposalAsync(board, proposalId)).Dates;
        await As(board).PostAsJsonAsync($"{V1}/interclubs/proposals/{proposalId}/choose", new { dateId = dates[0].Id }, CancellationToken);

        await As(board).DeleteAsync(Url(teamId, anna.Id), CancellationToken);

        Assert.Equal(1, (await ProposalAsync(board, proposalId)).Dates[0].Yes);
    }
}
