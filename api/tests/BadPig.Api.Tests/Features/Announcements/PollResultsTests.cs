using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class PollResultsTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_HideTheResultsFromMembersButShowParticipation_When_PollIsOpen()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, published: true);
        await GivenAnswerAsync(adherent, id, "Oui");

        var seenByAdherent = (await AnnouncementAsync(adherent, id)).Poll!;

        Assert.Equal(1, seenByAdherent.ParticipantCount);
        Assert.False(seenByAdherent.ResultsVisible);
        Assert.All(seenByAdherent.Options, o => Assert.Null(o.VoteCount));
        Assert.All(seenByAdherent.Options, o => Assert.Null(o.Voters));
    }

    [Fact]
    public async Task Should_ShowTheCountsToTheBoard_When_PollIsStillOpen()
    {
        var board = GivenBoardMember();
        var first = await GivenAnAdherentAsync(board, "Marie Adherente");
        var second = await GivenAnAdherentAsync(board, "Julie Adherente");
        var id = await GivenPollAsync(board, published: true);
        await GivenAnswerAsync(first, id, "Oui");
        await GivenAnswerAsync(second, id, "Oui");

        var poll = (await AnnouncementAsync(board, id)).Poll!;

        Assert.False(poll.IsClosed);
        Assert.True(poll.ResultsVisible);
        Assert.Equal(2, poll.Options.Single(o => o.Label == "Oui").VoteCount);
        Assert.Equal(0, poll.Options.Single(o => o.Label == "Non").VoteCount);
        Assert.Equal(["Julie Adherente", "Marie Adherente"], poll.Options.Single(o => o.Label == "Oui").Voters);
    }

    [Fact]
    public async Task Should_ShowTheBoardTheLatestAnswers_When_AMemberChangesTheirAnswerBeforeTheDeadline()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, published: true);
        await GivenAnswerAsync(adherent, id, "Oui");
        await GivenAnswerAsync(adherent, id, "Non");

        var poll = (await AnnouncementAsync(board, id)).Poll!;

        Assert.Equal(0, poll.Options.Single(o => o.Label == "Oui").VoteCount);
        Assert.Equal(1, poll.Options.Single(o => o.Label == "Non").VoteCount);
    }

    [Fact]
    public async Task Should_NeverShowWhoAnswered_When_PollIsAnonymousAndStillOpen()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, published: true, isAnonymous: true);
        await GivenAnswerAsync(adherent, id, "Oui");

        var poll = (await AnnouncementAsync(board, id)).Poll!;

        Assert.Equal(1, poll.Options.Single(o => o.Label == "Oui").VoteCount);
        Assert.All(poll.Options, o => Assert.Null(o.Voters));
    }

    [Fact]
    public async Task Should_ShowTheCountsToMembers_When_PollIsClosed()
    {
        var board = GivenBoardMember();
        var first = await GivenAnAdherentAsync(board, "Marie Adherente");
        var second = await GivenAnAdherentAsync(board, "Julie Adherente");
        var third = await GivenAnAdherentAsync(board, "Lucie Adherente");
        var id = await GivenPollAsync(board, published: true);
        await GivenAnswerAsync(first, id, "Oui");
        await GivenAnswerAsync(second, id, "Oui");
        await GivenAnswerAsync(third, id, "Non");
        GivenThePollIsClosed();

        var poll = (await AnnouncementAsync(first, id)).Poll!;

        Assert.True(poll.IsClosed);
        Assert.Equal(3, poll.ParticipantCount);
        Assert.Equal(2, poll.Options.Single(o => o.Label == "Oui").VoteCount);
        Assert.Equal(1, poll.Options.Single(o => o.Label == "Non").VoteCount);
        Assert.All(poll.Options, o => Assert.Null(o.Voters));
    }

    [Fact]
    public async Task Should_ShowWhoAnsweredWhatToTheBoardOnly_When_ClosedPollIsNotAnonymous()
    {
        var board = GivenBoardMember();
        var first = await GivenAnAdherentAsync(board, "Marie Adherente");
        var second = await GivenAnAdherentAsync(board, "Julie Adherente");
        var id = await GivenPollAsync(board, published: true);
        await GivenAnswerAsync(first, id, "Oui");
        await GivenAnswerAsync(second, id, "Oui");
        GivenThePollIsClosed();

        var seenByBoard = (await AnnouncementAsync(board, id)).Poll!;
        var seenByAdherent = (await AnnouncementAsync(first, id)).Poll!;

        Assert.Equal(["Julie Adherente", "Marie Adherente"], seenByBoard.Options.Single(o => o.Label == "Oui").Voters);
        Assert.Empty(seenByBoard.Options.Single(o => o.Label == "Non").Voters!);
        Assert.All(seenByAdherent.Options, o => Assert.Null(o.Voters));
    }

    [Fact]
    public async Task Should_NeverShowWhoAnsweredWhat_When_PollIsAnonymous()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, isAnonymous: true, published: true);
        await GivenAnswerAsync(adherent, id, "Oui");
        GivenThePollIsClosed();

        var poll = (await AnnouncementAsync(board, id)).Poll!;

        Assert.Equal(1, poll.Options.Single(o => o.Label == "Oui").VoteCount);
        Assert.All(poll.Options, o => Assert.Null(o.Voters));
    }

    [Fact]
    public async Task Should_ShowNoResultsToVisitors_When_PublicPollIsClosed()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, kind: "public", published: true);
        await GivenAnswerAsync(adherent, id, "Oui");
        GivenThePollIsClosed();

        var poll = (await AnnouncementAsync(null, id)).Poll!;

        Assert.True(poll.IsClosed);
        Assert.Null(poll.ParticipantCount);
        Assert.All(poll.Options, o => Assert.Null(o.VoteCount));
    }

    [Fact]
    public async Task Should_ReturnNoPoll_When_AnnouncementIsAText()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        Assert.Null((await AnnouncementAsync(board, id)).Poll);
        Assert.Equal("text", (await AnnouncementAsync(board, id)).Form);
        Assert.Equal("text", Assert.Single(await AnnouncementsAsync(board)).Form);
    }
}
