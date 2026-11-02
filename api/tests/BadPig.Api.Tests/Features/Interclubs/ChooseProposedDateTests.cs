using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class ChooseProposedDateTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string Url(string id) => $"{V1}/interclubs/proposals/{id}/choose";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync(Url(Guid.NewGuid().ToString()), new { dateId = Guid.NewGuid() }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_ProposalDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Url(Guid.NewGuid().ToString()), new { dateId = Guid.NewGuid() }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotTheCaptainOfTheTeam()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var proposalId = await GivenProposalAsync(board, await GivenTeamAsync(board));
        var dateId = (await ProposalAsync(board, proposalId)).Dates[0].Id;

        var response = await As(adherent).PostAsJsonAsync(Url(proposalId), new { dateId }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Empty(await FixturesAsync(board));
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_DateIsNotOneOfTheProposal()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);
        var proposalId = await GivenProposalAsync(board, teamId);
        var foreignDate = (await ProposalAsync(board, await GivenProposalAsync(board, teamId, "BC Nantes"))).Dates[0].Id;

        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PostAsJsonAsync(Url(proposalId), new { dateId = foreignDate }, CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PostAsJsonAsync(Url(proposalId), new { dateId = "nope" }, CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_ADateWasAlreadyChosen()
    {
        var board = GivenBoardMember();
        var proposalId = await GivenProposalAsync(board, await GivenTeamAsync(board));
        var dates = (await ProposalAsync(board, proposalId)).Dates;
        await As(board).PostAsJsonAsync(Url(proposalId), new { dateId = dates[0].Id }, CancellationToken);

        var response = await As(board).PostAsJsonAsync(Url(proposalId), new { dateId = dates[1].Id }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Single(await FixturesAsync(board));
    }

    [Fact]
    public async Task Should_CreateTheFixtureAndCloseTheProposal_When_TheCaptainChoosesADate()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var teamId = await GivenTeamAsync(board, "D4", captain: captain);
        var proposalId = await GivenProposalAsync(captain, teamId, "BC Lyon", "away", "2026-11-14", "2026-11-21");
        var dates = (await ProposalAsync(captain, proposalId)).Dates;

        var response = await As(captain).PostAsJsonAsync(Url(proposalId), new { dateId = dates[1].Id }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var fixtureId = (await ReadAsync<IdDto>(response)).Id;
        var fixture = await FixtureAsync(captain, fixtureId);
        Assert.Equal((teamId, "2026-11-21", "BC Lyon", "away"), (fixture.TeamId, fixture.Date, fixture.Opponent, fixture.Venue));
        var proposal = await ProposalAsync(captain, proposalId);
        Assert.False(proposal.IsOpen);
        Assert.Equal(fixtureId, proposal.FixtureId);
    }
}
