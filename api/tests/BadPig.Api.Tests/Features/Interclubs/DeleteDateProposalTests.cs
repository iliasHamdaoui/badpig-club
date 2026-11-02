using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class DeleteDateProposalTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{V1}/interclubs/proposals/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_ProposalDoesNotExist()
    {
        var response = await As(GivenBoardMember()).DeleteAsync($"{V1}/interclubs/proposals/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotTheCaptainOfTheTeam()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var proposalId = await GivenProposalAsync(board, await GivenTeamAsync(board));

        var response = await As(adherent).DeleteAsync($"{V1}/interclubs/proposals/{proposalId}", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotNull(await ProposalAsync(board, proposalId));
    }

    [Fact]
    public async Task Should_DeleteTheProposalAndKeepTheFixture_When_TheCaptainDeletesAChosenProposal()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var teamId = await GivenTeamAsync(board, captain: captain);
        var proposalId = await GivenProposalAsync(captain, teamId);
        await As(captain).PostAsJsonAsync($"{V1}/interclubs/proposals/{proposalId}/choose", new { dateId = (await ProposalAsync(captain, proposalId)).Dates[0].Id }, CancellationToken);

        var response = await As(captain).DeleteAsync($"{V1}/interclubs/proposals/{proposalId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await As(captain).GetAsync($"{V1}/interclubs/proposals/{proposalId}", CancellationToken)).StatusCode);
        Assert.Single(await FixturesAsync(captain));
    }

    [Fact]
    public async Task Should_DeleteTheProposal_When_ItsTeamIsDeleted()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);
        var proposalId = await GivenProposalAsync(board, teamId);

        await As(board).DeleteAsync($"{V1}/admin/interclubs/teams/{teamId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, (await As(board).GetAsync($"{V1}/interclubs/proposals/{proposalId}", CancellationToken)).StatusCode);
    }
}
