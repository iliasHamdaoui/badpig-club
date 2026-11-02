using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class AnswerDateProposalTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string Url(string id) => $"{V1}/interclubs/proposals/{id}/answers";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync(Url(Guid.NewGuid().ToString()), new { answers = new[] { new { dateId = Guid.NewGuid(), answer = "yes" } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_MembershipIsNotValidated()
    {
        var response = await As(GivenMember()).PutAsJsonAsync(Url(Guid.NewGuid().ToString()), new { answers = new[] { new { dateId = Guid.NewGuid(), answer = "yes" } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_ProposalDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PutAsJsonAsync(Url(Guid.NewGuid().ToString()), new { answers = new[] { new { dateId = Guid.NewGuid(), answer = "yes" } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_AnswerIsInvalidOrMissing()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);
        var proposalId = await GivenProposalAsync(board, teamId);
        await GivenPlayerAsync(board, teamId, board);
        var dateId = (await ProposalAsync(board, proposalId)).Dates[0].Id;

        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(Url(proposalId), new { answers = new[] { new { dateId, answer = "perhaps" } } }, CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(Url(proposalId), new { answers = Array.Empty<object>() }, CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(Url(proposalId), new { }, CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_DateIsNotOneOfTheProposal()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);
        var proposalId = await GivenProposalAsync(board, teamId);
        await GivenPlayerAsync(board, teamId, board);
        var otherProposal = await GivenProposalAsync(board, teamId, "BC Nantes");
        var foreignDate = (await ProposalAsync(board, otherProposal)).Dates[0].Id;

        var response = await As(board).PutAsJsonAsync(Url(proposalId), new { answers = new[] { new { dateId = foreignDate, answer = "yes" } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_ADateWasAlreadyChosen()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var teamId = await GivenTeamAsync(board);
        var proposalId = await GivenProposalAsync(board, teamId);
        await GivenPlayerAsync(board, teamId, adherent);
        var proposal = await ProposalAsync(board, proposalId);
        await As(board).PostAsJsonAsync($"{V1}/interclubs/proposals/{proposalId}/choose", new { dateId = proposal.Dates[0].Id }, CancellationToken);

        var response = await As(adherent).PutAsJsonAsync(Url(proposalId), new { answers = new[] { new { dateId = proposal.Dates[0].Id, answer = "yes" } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_RecordTheAnswers_When_AnAdherentAnswersSeveralDatesAtOnce()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var teamId = await GivenTeamAsync(board);
        var proposalId = await GivenProposalAsync(board, teamId);
        await GivenPlayerAsync(board, teamId, adherent);
        var dates = (await ProposalAsync(adherent, proposalId)).Dates;

        var response = await As(adherent).PutAsJsonAsync(Url(proposalId),
            new { answers = new[] { new { dateId = dates[0].Id, answer = "yes" }, new { dateId = dates[1].Id, answer = "no" } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var proposal = await ProposalAsync(adherent, proposalId);
        Assert.Equal(["yes", "no"], proposal.Dates.Select(d => d.MyAnswer));
    }

    [Fact]
    public async Task Should_ReplaceThePreviousAnswer_When_ThePersonAnswersAgain()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var teamId = await GivenTeamAsync(board);
        var proposalId = await GivenProposalAsync(board, teamId);
        await GivenPlayerAsync(board, teamId, adherent);
        await GivenAnswerAsync(adherent, proposalId, 0, "yes");

        await GivenAnswerAsync(adherent, proposalId, 0, "maybe");

        var date = (await ProposalAsync(adherent, proposalId)).Dates[0];
        Assert.Equal((0, 1, 0, "maybe"), (date.Yes, date.Maybe, date.No, date.MyAnswer));
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotAPlayerOfTheTeam()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var teamId = await GivenTeamAsync(board);
        var proposalId = await GivenProposalAsync(board, teamId);
        var dateId = (await ProposalAsync(board, proposalId)).Dates[0].Id;

        var response = await As(adherent).PutAsJsonAsync(Url(proposalId), new { answers = new[] { new { dateId, answer = "yes" } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(0, (await ProposalAsync(board, proposalId)).Dates[0].Yes);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonPlaysInAnotherTeamOnly()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var otherTeam = await GivenTeamAsync(board, "D5");
        await GivenPlayerAsync(board, otherTeam, adherent);
        var proposalId = await GivenProposalAsync(board, await GivenTeamAsync(board, "D4"));
        var dateId = (await ProposalAsync(board, proposalId)).Dates[0].Id;

        var response = await As(adherent).PutAsJsonAsync(Url(proposalId), new { answers = new[] { new { dateId, answer = "yes" } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
