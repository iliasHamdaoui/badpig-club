using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class WithdrawPollVoteTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{V1}/announcements/{Guid.NewGuid()}/poll/vote", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_MemberHasNotValidatedMembership()
    {
        var response = await As(GivenMember()).DeleteAsync($"{V1}/announcements/{Guid.NewGuid()}/poll/vote", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var response = await As(await GivenAnAdherentAsync(GivenBoardMember())).DeleteAsync($"{V1}/announcements/{Guid.NewGuid()}/poll/vote", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_RemoveMyAnswer_When_PollIsOpen()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, published: true);
        await GivenAnswerAsync(adherent, id, "Oui");

        var response = await As(adherent).DeleteAsync($"{V1}/announcements/{id}/poll/vote", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var poll = (await AnnouncementAsync(adherent, id)).Poll!;
        Assert.Empty(poll.MyOptionIds);
        Assert.Equal(0, poll.ParticipantCount);
    }

    [Fact]
    public async Task Should_ReturnNoContent_When_IHadNotAnswered()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, published: true);

        var response = await As(adherent).DeleteAsync($"{V1}/announcements/{id}/poll/vote", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflictAndKeepTheAnswer_When_PollIsClosed()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, published: true);
        await GivenAnswerAsync(adherent, id, "Oui");
        GivenThePollIsClosed();

        var response = await As(adherent).DeleteAsync($"{V1}/announcements/{id}/poll/vote", CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Single((await AnnouncementAsync(adherent, id)).Poll!.MyOptionIds);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementHasNoPoll()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);

        var response = await As(adherent).DeleteAsync($"{V1}/announcements/{id}/poll/vote", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
