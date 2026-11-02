using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class DeleteAnnouncementCommentTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{V1}/announcements/{Guid.NewGuid()}/comments/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_MembershipIsNotValidated()
    {
        var response = await As(GivenMember()).DeleteAsync($"{V1}/announcements/{Guid.NewGuid()}/comments/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_DeleteTheComment_When_AuthorDeletesTheirOwn()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        var commentId = await GivenCommentAsync(adherent, id);

        var response = await As(adherent).DeleteAsync($"{V1}/announcements/{id}/comments/{commentId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await CommentsAsync(adherent, id));
    }

    [Fact]
    public async Task Should_DeleteTheComment_When_BoardMemberDeletesAnotherPersonsComment()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        var commentId = await GivenCommentAsync(adherent, id);

        var response = await As(board).DeleteAsync($"{V1}/announcements/{id}/comments/{commentId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await CommentsAsync(board, id));
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_AdherentDeletesAnotherPersonsComment()
    {
        var board = GivenBoardMember();
        var author = await GivenAnAdherentAsync(board, "Marie Adherente");
        var other = await GivenAnAdherentAsync(board, "Julie Adherente");
        var id = await GivenAnnouncementAsync(board, published: true);
        var commentId = await GivenCommentAsync(author, id);

        var response = await As(other).DeleteAsync($"{V1}/announcements/{id}/comments/{commentId}", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Single(await CommentsAsync(author, id));
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_CommentDoesNotExist()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, published: true);

        var response = await As(board).DeleteAsync($"{V1}/announcements/{id}/comments/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_CommentBelongsToAnotherAnnouncement()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var first = await GivenAnnouncementAsync(board, "Une", published: true);
        var second = await GivenAnnouncementAsync(board, "Deux", published: true);
        var commentId = await GivenCommentAsync(adherent, first);

        var response = await As(adherent).DeleteAsync($"{V1}/announcements/{second}/comments/{commentId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Single(await CommentsAsync(adherent, first));
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_IdentifiersAreNotGuids()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);

        var response = await As(adherent).DeleteAsync($"{V1}/announcements/abc/comments/def", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_StillDelete_When_CommentsAreClosed()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        var commentId = await GivenCommentAsync(adherent, id);
        await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", new { title = "Titre", bodyHtml = "<p>Texte</p>", kind = "internal", commentsEnabled = false }, CancellationToken);

        var response = await As(adherent).DeleteAsync($"{V1}/announcements/{id}/comments/{commentId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Should_KeepTheAnswers_When_AQuotedCommentIsDeleted()
    {
        var board = GivenBoardMember();
        var author = await GivenAnAdherentAsync(board, "Marie Adherente");
        var other = await GivenAnAdherentAsync(board, "Julie Adherente");
        var id = await GivenAnnouncementAsync(board, published: true);
        var commentId = await GivenCommentAsync(author, id);
        await GivenCommentAsync(other, id, "Réponse", parentId: commentId);

        var response = await As(author).DeleteAsync($"{V1}/announcements/{id}/comments/{commentId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("Réponse", Assert.Single(await CommentsAsync(board, id)).Text);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_AdherentDeletesAnotherPersonsAnswer()
    {
        var board = GivenBoardMember();
        var author = await GivenAnAdherentAsync(board, "Marie Adherente");
        var other = await GivenAnAdherentAsync(board, "Julie Adherente");
        var id = await GivenAnnouncementAsync(board, published: true);
        var commentId = await GivenCommentAsync(author, id);
        var replyId = await GivenCommentAsync(other, id, "Réponse", parentId: commentId);

        var response = await As(author).DeleteAsync($"{V1}/announcements/{id}/comments/{replyId}", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
