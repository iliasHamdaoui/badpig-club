using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class UpdateAnnouncementTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static readonly object Edited = new { title = "Titre modifié", bodyHtml = "<p>Nouveau texte</p>", kind = "public" };

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync($"{V1}/admin/announcements/{Guid.NewGuid()}", Edited, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).PutAsJsonAsync($"{V1}/admin/announcements/{Guid.NewGuid()}", Edited, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PutAsJsonAsync($"{V1}/admin/announcements/{Guid.NewGuid()}", Edited, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReplaceTheContent_When_AnotherBoardMemberEditsTheAnnouncement()
    {
        var author = GivenBoardMember("Paul Bureau");
        var editor = GivenBoardMember("Bruno Bureau");
        var id = await GivenAnnouncementAsync(author);

        var response = await As(editor).PutAsJsonAsync($"{V1}/admin/announcements/{id}", Edited, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var announcement = await AnnouncementAsync(editor, id);
        Assert.Equal("Titre modifié", announcement.Title);
        Assert.Contains("Nouveau texte", announcement.BodyHtml);
        Assert.Equal("public", announcement.Kind);
        Assert.Equal("Paul Bureau", announcement.AuthorName);
    }

    [Fact]
    public async Task Should_KeepPublicationPinAndPoster_When_ContentIsEdited()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, published: true, pinned: true);
        await As(board).PutAsync($"{V1}/admin/announcements/{id}/poster", FileForm("affiche.png", PngBytes()), CancellationToken);

        await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", Edited, CancellationToken);

        var announcement = await AnnouncementAsync(board, id);
        Assert.True(announcement.IsPublished);
        Assert.True(announcement.IsPinned);
        Assert.NotNull(announcement.PublishedAt);
        Assert.Single(announcement.Files, f => f.IsPoster);
    }

    [Fact]
    public async Task Should_ShowTheAnnouncementToVisitors_When_KindBecomesPublic()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "internal", published: true);

        await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", Edited, CancellationToken);

        Assert.Contains(await AnnouncementsAsync(null), a => a.Id == id);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_AFieldIsInvalid()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", new { title = "", bodyHtml = "<p>Texte</p>", kind = "public" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ChangeTheEndDate_When_ANewOneIsGiven()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", new { title = "Titre", bodyHtml = "<p>Texte</p>", kind = "internal", expiresOn = "2027-06-30" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(new DateOnly(2027, 6, 30), (await AnnouncementAsync(board, id)).ExpiresOn);
    }

    [Fact]
    public async Task Should_KeepTheEndDate_When_NoneIsGiven()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", Edited, CancellationToken);

        Assert.Equal(new DateOnly(2027, 1, 13), (await AnnouncementAsync(board, id)).ExpiresOn);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheNewEndDateIsInThePast()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", new { title = "Titre", bodyHtml = "<p>Texte</p>", kind = "internal", expiresOn = "2026-10-01" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_EndDateIsBeforeThePollCloses()
    {
        var board = GivenBoardMember();
        var id = await GivenPollAsync(board, closesOn: "2026-12-01", published: true);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", new { title = "Titre", bodyHtml = "<p>Texte</p>", kind = "internal", expiresOn = "2026-11-01" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ChangeTheQuestionAndAcceptNoText_When_AnnouncementIsAPoll()
    {
        var board = GivenBoardMember();
        var id = await GivenPollAsync(board, published: true);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", new { title = "Nouvelle question ?", bodyHtml = "", kind = "public" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var poll = await AnnouncementAsync(board, id);
        Assert.Equal("Nouvelle question ?", poll.Title);
        Assert.Equal("public", poll.Kind);
        Assert.Equal(["Oui", "Non"], poll.Poll!.Options.Select(o => o.Label));
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TextAnnouncementHasNoText()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", new { title = "Titre", bodyHtml = "", kind = "internal" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_SwitchTheCommentsOffAndOn_When_CommentsEnabledIsGiven()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, published: true);

        await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", new { title = "Titre", bodyHtml = "<p>Texte</p>", kind = "internal", commentsEnabled = false }, CancellationToken);
        var closed = (await AnnouncementAsync(board, id)).CommentsEnabled;
        await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", new { title = "Titre", bodyHtml = "<p>Texte</p>", kind = "internal", commentsEnabled = true }, CancellationToken);

        Assert.False(closed);
        Assert.True((await AnnouncementAsync(board, id)).CommentsEnabled);
    }

    [Fact]
    public async Task Should_KeepTheCommentsChoice_When_NoneIsGiven()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, published: true, commentsEnabled: false);

        await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", Edited, CancellationToken);

        Assert.False((await AnnouncementAsync(board, id)).CommentsEnabled);
    }
}
