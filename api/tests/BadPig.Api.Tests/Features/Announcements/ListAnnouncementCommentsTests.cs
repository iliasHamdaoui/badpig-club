using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class ListAnnouncementCommentsTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync($"{V1}/announcements/{Guid.NewGuid()}/comments", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_MembershipIsNotValidated()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "public", published: true);

        var response = await As(GivenMember()).GetAsync($"{V1}/announcements/{id}/comments", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementIsNotPublished()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board);

        var response = await As(adherent).GetAsync($"{V1}/announcements/{id}/comments", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ListOldestFirst_When_NoSortIsGiven()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        await GivenCommentAsync(adherent, id, "Premier");
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 11, 0, 0, TimeSpan.Zero));
        await GivenCommentAsync(board, id, "Second");

        Assert.Equal(["Premier", "Second"], (await CommentsAsync(adherent, id)).Select(c => c.Text));
        Assert.Equal(["Second", "Premier"], (await CommentsAsync(adherent, id, "?direction=Desc")).Select(c => c.Text));
    }

    [Fact]
    public async Task Should_TellWhoCanDelete_When_AdherentReadsTheComments()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board, "Marie Adherente");
        var other = await GivenAnAdherentAsync(board, "Julie Adherente");
        var id = await GivenAnnouncementAsync(board, published: true);
        await GivenCommentAsync(adherent, id, "De Marie");
        await GivenCommentAsync(other, id, "De Julie");

        var comments = await CommentsAsync(adherent, id);

        var mine = comments.Single(c => c.Text == "De Marie");
        var theirs = comments.Single(c => c.Text == "De Julie");
        Assert.True(mine.IsMine);
        Assert.True(mine.CanDelete);
        Assert.False(theirs.IsMine);
        Assert.False(theirs.CanDelete);
    }

    [Fact]
    public async Task Should_AllowTheBoardToDeleteAnyComment_When_BoardReadsTheComments()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        await GivenCommentAsync(adherent, id);

        var comment = Assert.Single(await CommentsAsync(board, id));

        Assert.False(comment.IsMine);
        Assert.True(comment.CanDelete);
    }

    [Fact]
    public async Task Should_FilterOnTheText_When_SearchIsGiven()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        await GivenCommentAsync(adherent, id, "Je prends les volants");
        await GivenCommentAsync(adherent, id, "Je viens en voiture");

        var found = await CommentsAsync(adherent, id, "?search=VOLANTS");

        Assert.Equal("Je prends les volants", Assert.Single(found).Text);
    }

    [Fact]
    public async Task Should_PaginateTheComments_When_PageSizeIsGiven()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        for (var i = 1; i <= 3; i++)
            await GivenCommentAsync(adherent, id, $"Commentaire {i}");

        var page = await ReadAsync<PageDto<CommentDto>>(await As(adherent).GetAsync($"{V1}/announcements/{id}/comments?pageSize=2&page=2", CancellationToken));

        Assert.Equal(3, page.TotalCount);
        Assert.Single(page.Items);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_SortIsUnknown()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);

        var response = await As(adherent).GetAsync($"{V1}/announcements/{id}/comments?sort=author", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_KeepListingTheComments_When_CommentsAreClosed()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        await GivenCommentAsync(adherent, id);
        await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", new { title = "Titre", bodyHtml = "<p>Texte</p>", kind = "internal", commentsEnabled = false }, CancellationToken);

        Assert.Single(await CommentsAsync(adherent, id));
        Assert.False((await AnnouncementAsync(adherent, id)).CommentsEnabled);
    }

    [Fact]
    public async Task Should_ListAnswersAsPartOfTheConversation_When_CommentsHaveAnswers()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board, "Marie Adherente");
        var other = await GivenAnAdherentAsync(board, "Julie Adherente");
        var id = await GivenAnnouncementAsync(board, published: true);
        var first = await GivenCommentAsync(adherent, id, "Premier");
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 11, 0, 0, TimeSpan.Zero));
        await GivenCommentAsync(other, id, "Réponse", parentId: first);

        var page = await ReadAsync<PageDto<CommentDto>>(await As(adherent).GetAsync($"{V1}/announcements/{id}/comments", CancellationToken));

        Assert.Equal(2, page.TotalCount);
        Assert.Equal(["Premier", "Réponse"], page.Items.Select(c => c.Text));
    }

    [Fact]
    public async Task Should_QuoteACommentOfAnotherPage_When_AnswerIsOnTheSecondPage()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        var first = await GivenCommentAsync(adherent, id, "Premier");
        for (var i = 1; i <= 2; i++)
        {
            Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 10 + i, 0, 0, TimeSpan.Zero));
            await GivenCommentAsync(adherent, id, $"Intermédiaire {i}");
        }

        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 14, 0, 0, TimeSpan.Zero));
        await GivenCommentAsync(adherent, id, "Réponse", parentId: first);

        var page = await ReadAsync<PageDto<CommentDto>>(await As(adherent).GetAsync($"{V1}/announcements/{id}/comments?pageSize=2&page=2", CancellationToken));

        Assert.Equal("Premier", page.Items.Last().ReplyTo!.Excerpt);
    }

    [Fact]
    public async Task Should_ShowTheQuoteAsDeleted_When_QuotedCommentIsDeleted()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        var first = await GivenCommentAsync(adherent, id, "Premier");
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 11, 0, 0, TimeSpan.Zero));
        await GivenCommentAsync(adherent, id, "Réponse", parentId: first);
        await As(adherent).DeleteAsync($"{V1}/announcements/{id}/comments/{first}", CancellationToken);

        var comment = Assert.Single(await CommentsAsync(adherent, id));

        Assert.Equal("Réponse", comment.Text);
        Assert.True(comment.ReplyTo!.IsDeleted);
        Assert.Null(comment.ReplyTo.AuthorName);
        Assert.Equal("", comment.ReplyTo.Excerpt);
    }
}
