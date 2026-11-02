using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class GetDateProposalTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync($"{V1}/interclubs/proposals/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_MembershipIsNotValidated()
    {
        var response = await As(GivenMember()).GetAsync($"{V1}/interclubs/proposals/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_ProposalDoesNotExist()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{V1}/interclubs/proposals/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_IdentifierIsInvalid()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{V1}/interclubs/proposals/not-an-identifier", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_NotGiveTheNames_When_AnOrdinaryAdherentReadsIt()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var teamId = await GivenTeamAsync(board);
        var proposalId = await GivenProposalAsync(board, teamId);
        await GivenPlayerAsync(board, teamId, adherent);
        await GivenAnswerAsync(adherent, proposalId, 0, "yes");

        var proposal = await ProposalAsync(adherent, proposalId);

        Assert.False(proposal.CanManage);
        Assert.True(proposal.CanAnswer);
        Assert.Equal(1, proposal.Dates[0].Yes);
        Assert.Empty(proposal.Dates[0].Answers!);
    }

    [Fact]
    public async Task Should_GiveWhoAnsweredWhat_When_TheCaptainReadsIt()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var anna = await GivenAnAdherentAsync(board, "Anna Adherente");
        var bob = await GivenAnAdherentAsync(board, "Bob Adherent");
        var teamId = await GivenTeamAsync(board, captain: captain);
        var proposalId = await GivenProposalAsync(captain, teamId);
        await GivenPlayerAsync(captain, teamId, anna);
        await GivenPlayerAsync(captain, teamId, bob);
        await GivenAnswerAsync(bob, proposalId, 0, "no");
        await GivenAnswerAsync(anna, proposalId, 0, "yes");

        var proposal = await ProposalAsync(captain, proposalId);

        Assert.True(proposal.CanManage);
        var answers = proposal.Dates[0].Answers!;
        Assert.Equal(["Anna Adherente", "Bob Adherent"], answers.Select(a => a.Name));
        Assert.Equal(["yes", "no"], answers.Select(a => a.Answer));
        Assert.Empty(proposal.Dates[1].Answers!);
    }

    [Fact]
    public async Task Should_NotAllowToAnswer_When_PersonIsNotAPlayerOrTheProposalIsClosed()
    {
        var board = GivenBoardMember();
        var player = await GivenAnAdherentAsync(board, "Anna Adherente");
        var outsider = await GivenAnAdherentAsync(board, "Bob Adherent");
        var teamId = await GivenTeamAsync(board);
        var proposalId = await GivenProposalAsync(board, teamId);
        await GivenPlayerAsync(board, teamId, player);

        Assert.False((await ProposalAsync(outsider, proposalId)).CanAnswer);
        Assert.True((await ProposalAsync(player, proposalId)).CanAnswer);

        var dateId = (await ProposalAsync(board, proposalId)).Dates[0].Id;
        await As(board).PostAsJsonAsync($"{V1}/interclubs/proposals/{proposalId}/choose", new { dateId }, CancellationToken);
        Assert.False((await ProposalAsync(player, proposalId)).CanAnswer);
    }
}
