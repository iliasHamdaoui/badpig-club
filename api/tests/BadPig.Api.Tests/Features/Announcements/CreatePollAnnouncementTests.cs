using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class CreatePollAnnouncementTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/admin/announcements/polls";

    private static object Poll(object? options = null, string? closesOn = "2026-10-20", string? title = "On se retrouve ?", string kind = "internal",
        string? bodyHtml = null, string? expiresOn = null) =>
        new { title, bodyHtml, kind, options = options ?? new[] { "Oui", "Non" }, allowMultiple = false, isAnonymous = false, closesOn, expiresOn };

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync(Route, Poll(), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).PostAsJsonAsync(Route, Poll(), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_CreateAPollAnnouncementAsADraft_When_BoardCreatesAPoll()
    {
        var board = GivenBoardMember();

        var id = await GivenPollAsync(board, ["Samedi", "Dimanche", "Peu importe"], title: "Quel jour pour le tournoi ?", allowMultiple: true, isAnonymous: true);

        var poll = await AnnouncementAsync(board, id);
        Assert.Equal("poll", poll.Form);
        Assert.Equal("Quel jour pour le tournoi ?", poll.Title);
        Assert.False(poll.IsPublished);
        Assert.Equal(["Samedi", "Dimanche", "Peu importe"], poll.Poll!.Options.Select(o => o.Label));
        Assert.True(poll.Poll.AllowMultiple);
        Assert.True(poll.Poll.IsAnonymous);
        Assert.Equal(new DateOnly(2026, 10, 20), poll.Poll.ClosesOn);
        Assert.Equal("poll", Assert.Single(await AnnouncementsAsync(board)).Form);
    }

    [Fact]
    public async Task Should_AcceptAPollWithoutText_When_NoExplanationIsGiven()
    {
        var board = GivenBoardMember();

        var id = await GivenPollAsync(board, published: true);

        Assert.Equal("", (await AnnouncementAsync(board, id)).BodyHtml);
    }

    [Fact]
    public async Task Should_KeepTheExplanation_When_ATextIsGiven()
    {
        var board = GivenBoardMember();

        var response = await As(board).PostAsJsonAsync(Route, Poll(bodyHtml: "<p>Merci de répondre vite.</p>"), CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = (await ReadAsync<IdDto>(response)).Id;
        Assert.Contains("Merci de répondre vite.", (await AnnouncementAsync(board, id)).BodyHtml);
    }

    [Fact]
    public async Task Should_ShowThePollWithoutResultsToVisitors_When_PollIsPublic()
    {
        var board = GivenBoardMember();
        var id = await GivenPollAsync(board, kind: "public", published: true);

        var poll = (await AnnouncementAsync(null, id)).Poll!;

        Assert.Equal(2, poll.Options.Count);
        Assert.False(poll.CanVote);
        Assert.Null(poll.ParticipantCount);
        Assert.All(poll.Options, o => Assert.Null(o.VoteCount));
    }

    [Fact]
    public async Task Should_KeepThePollDisplayedUntilItCloses_When_ClosingDateIsAfterTheDefaultEnd()
    {
        var board = GivenBoardMember();
        var id = await GivenPollAsync(board, closesOn: "2027-03-01", published: true);

        Assert.Equal(new DateOnly(2027, 3, 1), (await AnnouncementAsync(board, id)).ExpiresOn);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_EndDateIsBeforeTheClosingDate()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, Poll(closesOn: "2026-12-01", expiresOn: "2026-11-01"), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_QuestionIsMissing()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, Poll(title: null), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_KindIsUnknown()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, Poll(kind: "secret"), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_OptionsAreMissing()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, new { title = "Question", kind = "internal", closesOn = "2026-10-20" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(CancellationToken)).GetProperty("errors");
        Assert.True(errors.TryGetProperty("options", out _));
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_OnlyOneOptionIsGiven()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, Poll(new[] { "Seul" }), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_MoreThanTenOptionsAreGiven()
    {
        var options = Enumerable.Range(1, 11).Select(i => $"Choix {i}").ToArray();

        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, Poll(options), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TwoOptionsHaveTheSameText()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, Poll(new[] { "Oui", " oui " }), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_AnOptionIsEmptyOrTooLong()
    {
        var board = GivenBoardMember();

        var empty = await As(board).PostAsJsonAsync(Route, Poll(new[] { "Oui", " " }), CancellationToken);
        var tooLong = await As(board).PostAsJsonAsync(Route, Poll(new[] { "Oui", new string('x', 101) }), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequestOnTheClosingDate_When_ItIsMissing()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, Poll(closesOn: null), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(CancellationToken)).GetProperty("errors");
        Assert.True(errors.TryGetProperty("closesOn", out _));
    }

    [Fact]
    public async Task Should_ReturnBadRequestOnTheClosingDate_When_ItIsInThePast()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, Poll(closesOn: "2026-10-14"), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_CloseTheComments_When_AuthorTurnsThemOffOnAPoll()
    {
        var board = GivenBoardMember();

        var response = await As(board).PostAsJsonAsync(Route, new { title = "Question", kind = "internal", options = new[] { "Oui", "Non" }, closesOn = "2026-10-20", commentsEnabled = false }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.False((await AnnouncementAsync(board, (await ReadAsync<IdDto>(response)).Id)).CommentsEnabled);
    }

    [Fact]
    public async Task Should_OpenTheComments_When_NoChoiceIsGivenOnAPoll()
    {
        var board = GivenBoardMember();

        var id = await GivenPollAsync(board);

        Assert.True((await AnnouncementAsync(board, id)).CommentsEnabled);
    }
}
