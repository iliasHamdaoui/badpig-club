using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class VoteInPollTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync($"{V1}/announcements/{Guid.NewGuid()}/poll/vote", new { optionIds = new[] { Guid.NewGuid().ToString() } }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_MemberHasNotValidatedMembership()
    {
        var response = await As(GivenMember()).PutAsJsonAsync($"{V1}/announcements/{Guid.NewGuid()}/poll/vote", new { optionIds = new[] { Guid.NewGuid().ToString() } }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var response = await As(await GivenAnAdherentAsync(GivenBoardMember())).PutAsJsonAsync($"{V1}/announcements/{Guid.NewGuid()}/poll/vote", new { optionIds = new[] { Guid.NewGuid().ToString() } }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementHasNoPoll()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);

        var response = await As(adherent).PutAsJsonAsync($"{V1}/announcements/{id}/poll/vote", new { optionIds = new[] { Guid.NewGuid().ToString() } }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementIsNotPublished()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board);
        var optionId = OptionId(await AnnouncementAsync(board, id), "Oui");

        var response = await As(adherent).PutAsJsonAsync($"{V1}/announcements/{id}/poll/vote", new { optionIds = new[] { optionId } }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_RecordTheAnswer_When_AdherentAnswersAnOpenPoll()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, published: true);

        await GivenAnswerAsync(adherent, id, "Oui");

        var poll = (await AnnouncementAsync(adherent, id)).Poll!;
        Assert.Equal(1, poll.ParticipantCount);
        Assert.Equal([OptionId(await AnnouncementAsync(adherent, id), "Oui")], poll.MyOptionIds);
        Assert.True(poll.CanVote);
    }

    [Fact]
    public async Task Should_ReplaceThePreviousAnswer_When_MemberChangesTheirMind()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, published: true);
        await GivenAnswerAsync(adherent, id, "Oui");

        await GivenAnswerAsync(adherent, id, "Non");

        var announcement = await AnnouncementAsync(adherent, id);
        Assert.Equal([OptionId(announcement, "Non")], announcement.Poll!.MyOptionIds);
        Assert.Equal(1, announcement.Poll.ParticipantCount);
    }

    [Fact]
    public async Task Should_RecordSeveralChoices_When_PollAllowsMultipleAnswers()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, ["Samedi", "Dimanche", "Lundi"], allowMultiple: true, published: true);

        await GivenAnswerAsync(adherent, id, "Samedi", "Lundi");

        var announcement = await AnnouncementAsync(adherent, id);
        Assert.Equivalent(new[] { OptionId(announcement, "Samedi"), OptionId(announcement, "Lundi") }, announcement.Poll!.MyOptionIds);
        Assert.Equal(1, announcement.Poll.ParticipantCount);
    }

    [Fact]
    public async Task Should_LetTheBoardAnswer_When_BoardMemberVotes()
    {
        var board = GivenBoardMember();
        var id = await GivenPollAsync(board, published: true);

        await GivenAnswerAsync(board, id, "Non");

        Assert.Equal(1, (await AnnouncementAsync(board, id)).Poll!.ParticipantCount);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_SeveralChoicesAreGivenToASingleAnswerPoll()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, published: true);
        var announcement = await AnnouncementAsync(adherent, id);

        var response = await As(adherent).PutAsJsonAsync($"{V1}/announcements/{id}/poll/vote",
            new { optionIds = new[] { OptionId(announcement, "Oui"), OptionId(announcement, "Non") } }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_NoChoiceIsGiven()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, published: true);

        var response = await As(adherent).PutAsJsonAsync($"{V1}/announcements/{id}/poll/vote", new { optionIds = Array.Empty<string>() }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_ChoiceBelongsToAnotherPoll()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var first = await GivenPollAsync(board, title: "Une", published: true);
        var second = await GivenPollAsync(board, title: "Deux", published: true);
        var foreignOption = OptionId(await AnnouncementAsync(adherent, second), "Oui");

        var response = await As(adherent).PutAsJsonAsync($"{V1}/announcements/{first}/poll/vote", new { optionIds = new[] { foreignOption } }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheSameChoiceIsGivenTwice()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, allowMultiple: true, published: true);
        var oui = OptionId(await AnnouncementAsync(adherent, id), "Oui");

        var response = await As(adherent).PutAsJsonAsync($"{V1}/announcements/{id}/poll/vote", new { optionIds = new[] { oui, oui } }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_StillAccept_When_TodayIsTheClosingDate()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, published: true);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 20, 20, 0, 0, TimeSpan.Zero));

        await GivenAnswerAsync(adherent, id, "Oui");
    }

    [Fact]
    public async Task Should_ReturnConflictAndKeepTheAnswer_When_PollIsClosed()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, published: true);
        await GivenAnswerAsync(adherent, id, "Oui");
        var announcement = await AnnouncementAsync(adherent, id);
        GivenThePollIsClosed();

        var response = await As(adherent).PutAsJsonAsync($"{V1}/announcements/{id}/poll/vote", new { optionIds = new[] { OptionId(announcement, "Non") } }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var closed = (await AnnouncementAsync(adherent, id)).Poll!;
        Assert.Equal([OptionId(announcement, "Oui")], closed.MyOptionIds);
        Assert.False(closed.CanVote);
    }
}
