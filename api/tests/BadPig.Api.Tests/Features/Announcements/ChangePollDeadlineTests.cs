using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class ChangePollDeadlineTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/poll/deadline", new { closesOn = "2026-10-30" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).PutAsJsonAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/poll/deadline", new { closesOn = "2026-10-30" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementHasNoPoll()
    {
        var response = await As(GivenBoardMember()).PutAsJsonAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/poll/deadline", new { closesOn = "2026-10-30" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_MoveTheClosingDateAndKeepTheAnswers_When_PollIsOpen()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, published: true);
        await GivenAnswerAsync(adherent, id, "Oui");

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}/poll/deadline", new { closesOn = "2026-10-30" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var poll = (await AnnouncementAsync(adherent, id)).Poll!;
        Assert.Equal(new DateOnly(2026, 10, 30), poll.ClosesOn);
        Assert.Equal(1, poll.ParticipantCount);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_PollIsAlreadyClosed()
    {
        var board = GivenBoardMember();
        var id = await GivenPollAsync(board, published: true);
        GivenThePollIsClosed();

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}/poll/deadline", new { closesOn = "2026-10-30" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_NewDateIsInThePast()
    {
        var board = GivenBoardMember();
        var id = await GivenPollAsync(board, published: true);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}/poll/deadline", new { closesOn = "2026-10-14" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_NewDateIsMissing()
    {
        var board = GivenBoardMember();
        var id = await GivenPollAsync(board, published: true);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}/poll/deadline", new { }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_KeepTheAnnouncementDisplayedUntilThePollCloses_When_DeadlineGoesPastItsEndDate()
    {
        var board = GivenBoardMember();
        var id = await GivenPollAsync(board, published: true);

        await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}/poll/deadline", new { closesOn = "2027-05-01" }, CancellationToken);

        Assert.Equal(new DateOnly(2027, 5, 1), (await AnnouncementAsync(board, id)).ExpiresOn);
    }
}
