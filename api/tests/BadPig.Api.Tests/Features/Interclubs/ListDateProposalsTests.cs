using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class ListDateProposalsTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string Url => $"{V1}/interclubs/proposals";

    private async Task<List<ProposalDto>> ProposalsAsync(Person reader, string query = "") =>
        (await ReadAsync<PageDto<ProposalDto>>(await As(reader).GetAsync(Url + query, CancellationToken))).Items;

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
    public async Task Should_ReturnBadRequest_When_SortStatusOrTeamIsInvalid()
    {
        var board = GivenBoardMember();

        foreach (var query in new[] { "?sort=score", "?status=closed", "?teamId=not-an-identifier" })
            Assert.Equal(HttpStatusCode.BadRequest, (await As(board).GetAsync(Url + query, CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_ListTheProposalsWithTheCountsAndMyAnswer_When_AnAdherentAsks()
    {
        var board = GivenBoardMember();
        var me = await GivenAnAdherentAsync(board);
        var other = await GivenAnAdherentAsync(board, "Denis Deuxieme");
        var teamId = await GivenTeamAsync(board, "D4");
        var proposalId = await GivenProposalAsync(board, teamId);
        await GivenPlayerAsync(board, teamId, me);
        await GivenPlayerAsync(board, teamId, other);
        await GivenAnswerAsync(me, proposalId, 0, "yes");
        await GivenAnswerAsync(other, proposalId, 0, "no");
        await GivenAnswerAsync(other, proposalId, 1, "maybe");

        var proposal = Assert.Single(await ProposalsAsync(me));

        Assert.Equal("D4", proposal.TeamName);
        Assert.False(proposal.CanManage);
        Assert.True(proposal.CanAnswer);
        var first = proposal.Dates[0];
        Assert.Equal((1, 0, 1, "yes"), (first.Yes, first.Maybe, first.No, first.MyAnswer));
        var second = proposal.Dates[1];
        Assert.Equal((0, 1, 0, (string?)null), (second.Yes, second.Maybe, second.No, second.MyAnswer));
    }

    [Fact]
    public async Task Should_SplitOpenAndChosenProposals_When_StatusIsGiven()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);
        var open = await GivenProposalAsync(board, teamId, "BC Lyon");
        var chosen = await GivenProposalAsync(board, teamId, "BC Nantes");
        var date = (await ProposalAsync(board, chosen)).Dates[0].Id;
        await As(board).PostAsJsonAsync($"{V1}/interclubs/proposals/{chosen}/choose", new { dateId = date }, CancellationToken);

        Assert.Equal(open, Assert.Single(await ProposalsAsync(board, "?status=open")).Id);
        Assert.Equal(chosen, Assert.Single(await ProposalsAsync(board, "?status=chosen")).Id);
        Assert.Equal(2, (await ProposalsAsync(board)).Count);
    }

    [Fact]
    public async Task Should_FilterOnTheTeamAndSearch()
    {
        var board = GivenBoardMember();
        var d4 = await GivenTeamAsync(board, "D4");
        var d5 = await GivenTeamAsync(board, "D5");
        await GivenProposalAsync(board, d4, "BC Lyon");
        await GivenProposalAsync(board, d5, "BC Nantes");

        Assert.Equal("BC Nantes", Assert.Single(await ProposalsAsync(board, $"?teamId={d5}")).Opponent);
        Assert.Equal("BC Lyon", Assert.Single(await ProposalsAsync(board, "?search=lyon")).Opponent);
        Assert.Equal("BC Lyon", Assert.Single(await ProposalsAsync(board, "?search=d4")).Opponent);
    }

    [Fact]
    public async Task Should_ShowTheLatestFirst_When_NoSortIsGiven()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);
        await GivenProposalAsync(board, teamId, "BC Lyon");
        Api.Clock.UtcNow.Returns(Api.Clock.UtcNow.AddHours(1));
        await GivenProposalAsync(board, teamId, "BC Nantes");

        var proposals = await ProposalsAsync(board);

        Assert.Equal(["BC Nantes", "BC Lyon"], proposals.Select(p => p.Opponent));
    }
}
