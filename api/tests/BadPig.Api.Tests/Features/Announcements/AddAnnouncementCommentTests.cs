using BadPig.Shared.Core;
using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class AddAnnouncementCommentTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync($"{V1}/announcements/{Guid.NewGuid()}/comments", new { text = "Bonjour" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_MembershipIsNotValidated()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "public", published: true);

        var response = await As(GivenMember()).PostAsJsonAsync($"{V1}/announcements/{id}/comments", new { text = "Bonjour" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_AddTheComment_When_AdherentComments()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board, "Marie Adherente");
        var id = await GivenAnnouncementAsync(board, published: true);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/announcements/{id}/comments", new { text = "  Je viens avec Paul !  " }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var comment = Assert.Single(await CommentsAsync(adherent, id));
        Assert.Equal((await ReadAsync<IdDto>(response)).Id, comment.Id);
        Assert.Equal("Marie Adherente", comment.AuthorName);
        Assert.Equal("Je viens avec Paul !", comment.Text);
    }

    [Fact]
    public async Task Should_AddTheComment_When_BoardMemberComments()
    {
        var board = GivenBoardMember();
        var id = await GivenPollAsync(board, published: true);

        await GivenCommentAsync(board, id, "Votez nombreux");

        Assert.Single(await CommentsAsync(board, id));
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/announcements/{Guid.NewGuid()}/comments", new { text = "Bonjour" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementIsNotPublished()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/announcements/{id}/comments", new { text = "Bonjour" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementIsExpired()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2027, 2, 1, 10, 0, 0, TimeSpan.Zero));

        var response = await As(adherent).PostAsJsonAsync($"{V1}/announcements/{id}/comments", new { text = "Bonjour" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_CommentIsEmpty()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);

        var empty = await As(adherent).PostAsJsonAsync($"{V1}/announcements/{id}/comments", new { text = "" }, CancellationToken);
        var blank = await As(adherent).PostAsJsonAsync($"{V1}/announcements/{id}/comments", new { text = "   " }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, blank.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_CommentIsTooLong()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/announcements/{id}/comments", new { text = new string('x', 1001) }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_AnnouncementIdIsNotAGuid()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/announcements/abc/comments", new { text = "Bonjour" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_CommentToAnswerDoesNotExist()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/announcements/{id}/comments", new { text = "Réponse", parentId = Guid.NewGuid() }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_CommentToAnswerIsOnAnotherAnnouncement()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var first = await GivenAnnouncementAsync(board, "Une", published: true);
        var second = await GivenAnnouncementAsync(board, "Deux", published: true);
        var commentId = await GivenCommentAsync(adherent, first);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/announcements/{second}/comments", new { text = "Réponse", parentId = commentId }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_ParentIdIsNotAGuid()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/announcements/{id}/comments", new { text = "Réponse", parentId = "abc" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_CommentsAreClosed()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true, commentsEnabled: false);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/announcements/{id}/comments", new { text = "Bonjour" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Empty(await CommentsAsync(adherent, id));
    }

    [Fact]
    public async Task Should_ReturnConflict_When_CommentsAreClosedAfterAFirstComment()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        var commentId = await GivenCommentAsync(adherent, id);
        await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", new { title = "Titre", bodyHtml = "<p>Texte</p>", kind = "internal", commentsEnabled = false }, CancellationToken);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/announcements/{id}/comments", new { text = "Réponse", parentId = commentId }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_BeVisibleToEveryAdherent_When_AnAdherentComments()
    {
        var board = GivenBoardMember();
        var author = await GivenAnAdherentAsync(board, "Marie Adherente");
        var reader = await GivenAnAdherentAsync(board, "Julie Adherente");
        var id = await GivenAnnouncementAsync(board, published: true);
        await GivenCommentAsync(author, id, "Bonjour à tous");

        var seen = Assert.Single(await CommentsAsync(reader, id));

        Assert.Equal("Bonjour à tous", seen.Text);
        Assert.Equal("Marie Adherente", seen.AuthorName);
    }

    [Fact]
    public async Task Should_QuoteTheComment_When_MemberAnswersAComment()
    {
        var board = GivenBoardMember();
        var author = await GivenAnAdherentAsync(board, "Marie Adherente");
        var other = await GivenAnAdherentAsync(board, "Julie Adherente");
        var id = await GivenAnnouncementAsync(board, published: true);
        var commentId = await GivenCommentAsync(author, id, "Qui apporte les volants ?");
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 11, 0, 0, TimeSpan.Zero));

        await GivenCommentAsync(other, id, "Moi !", parentId: commentId);

        var comments = await CommentsAsync(author, id);
        Assert.Null(comments[0].ReplyTo);
        var quote = comments[1].ReplyTo!;
        Assert.Equal(commentId, quote.Id);
        Assert.Equal("Marie Adherente", quote.AuthorName);
        Assert.Equal("Qui apporte les volants ?", quote.Excerpt);
        Assert.False(quote.IsDeleted);
    }

    [Fact]
    public async Task Should_QuoteAnAnswer_When_MemberAnswersAnAnswer()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        var commentId = await GivenCommentAsync(adherent, id, "Premier");
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 11, 0, 0, TimeSpan.Zero));
        var replyId = await GivenCommentAsync(adherent, id, "Réponse", parentId: commentId);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 12, 0, 0, TimeSpan.Zero));

        await GivenCommentAsync(adherent, id, "Re-réponse", parentId: replyId);

        var last = (await CommentsAsync(adherent, id)).Last();
        Assert.Equal(replyId, last.ReplyTo!.Id);
        Assert.Equal("Réponse", last.ReplyTo.Excerpt);
    }

    [Fact]
    public async Task Should_ShortenTheQuote_When_QuotedCommentIsLong()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        var commentId = await GivenCommentAsync(adherent, id, new string('a', 500));
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 15, 11, 0, 0, TimeSpan.Zero));
        await GivenCommentAsync(adherent, id, "Réponse", parentId: commentId);

        var quote = (await CommentsAsync(adherent, id)).Last().ReplyTo!;

        Assert.True(quote.Excerpt.Length <= 121);
        Assert.True(quote.Excerpt.EndsWith('…'));
    }

    // --- E-mails ---

    // The comment e-mails all have "commentaire" in their subject (other e-mails of the tests, such as a membership, do not).
    private void AssertCommentEmails(Person to, int times, string? subjectContains = null, string? bodyContains = null) =>
        _ = Api.Mail.Received(times).SendAsync(
            Arg.Is<AccountInfo?>(a => a != null && a.Id == to.Id),
            Arg.Is<string>(s => s.Contains("commentaire") && (subjectContains == null || s.Contains(subjectContains))),
            Arg.Is<string>(b => bodyContains == null || b.Contains(bodyContains)),
            Arg.Any<CancellationToken>());

    [Fact]
    public async Task Should_EmailTheAuthorOfTheAnnouncementWithTheComment_When_AdherentComments()
    {
        var board = GivenBoardMember("Paul Bureau");
        var adherent = await GivenAnAdherentAsync(board, "Marie Adherente");
        var id = await GivenAnnouncementAsync(board, "Tournoi interne", published: true);

        await GivenCommentAsync(adherent, id, "Je viens avec Paul !");

        AssertCommentEmails(board, 1, "Nouveau commentaire sur « Tournoi interne »", "Marie Adherente a commenté votre annonce « Tournoi interne »");
        AssertCommentEmails(board, 1, bodyContains: "Je viens avec Paul !");
        AssertCommentEmails(board, 1, bodyContains: $"https://test.badpig.fr/announcements/{id}");
    }

    [Fact]
    public async Task Should_CallItAPoll_When_ThePollIsCommented()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, published: true);

        await GivenCommentAsync(adherent, id, "Plutôt samedi");

        AssertCommentEmails(board, 1, bodyContains: "votre sondage");
    }

    [Fact]
    public async Task Should_NotEmailAnyone_When_AuthorCommentsTheirOwnAnnouncement()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, published: true);

        await GivenCommentAsync(board, id, "Précision : rendez-vous à 14 h.");

        _ = Api.Mail.DidNotReceive().SendAsync(Arg.Any<AccountInfo?>(), Arg.Is<string>(s => s.Contains("commentaire")), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_EmailThePersonAnswered_When_MemberAnswersAComment()
    {
        var board = GivenBoardMember();
        var author = await GivenAnAdherentAsync(board, "Marie Adherente");
        var other = await GivenAnAdherentAsync(board, "Julie Adherente");
        var id = await GivenAnnouncementAsync(board, "Tournoi interne", published: true);
        var commentId = await GivenCommentAsync(author, id, "Qui apporte les volants ?");

        await GivenCommentAsync(other, id, "Moi !", parentId: commentId);

        AssertCommentEmails(author, 1, "Julie Adherente a répondu à votre commentaire", "Julie Adherente a répondu à votre commentaire sous « Tournoi interne »");
        AssertCommentEmails(author, 1, bodyContains: "Moi !");
        AssertCommentEmails(author, 1, bodyContains: "Votre commentaire : Qui apporte les volants ?");
        AssertCommentEmails(board, 2); // the first comment, then the answer: the author of the announcement is told of both
    }

    [Fact]
    public async Task Should_SendASingleEmail_When_AnsweredPersonIsTheAuthorOfTheAnnouncement()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board, "Marie Adherente");
        var id = await GivenAnnouncementAsync(board, published: true);
        var commentId = await GivenCommentAsync(board, id, "Des questions ?");

        await GivenCommentAsync(adherent, id, "Oui, à quelle heure ?", parentId: commentId);

        AssertCommentEmails(board, 1);
        AssertCommentEmails(board, 1, "Marie Adherente a répondu à votre commentaire");
    }

    [Fact]
    public async Task Should_NotEmailTheWriter_When_MemberAnswersTheirOwnComment()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        var commentId = await GivenCommentAsync(adherent, id, "Premier message");

        await GivenCommentAsync(adherent, id, "J'ajoute une précision", parentId: commentId);

        AssertCommentEmails(adherent, 0);
        AssertCommentEmails(board, 2);
    }

    [Fact]
    public async Task Should_AddTheComment_When_TheEmailCannotBeSent()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        Api.Mail.SendAsync(Arg.Any<AccountInfo?>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("mail server down")));

        var response = await As(adherent).PostAsJsonAsync($"{V1}/announcements/{id}/comments", new { text = "Bonjour" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Single(await CommentsAsync(adherent, id));
    }

    [Fact]
    public async Task Should_AddTheCommentWithoutEmail_When_TheAuthorAccountWasDeleted()
    {
        var author = GivenBoardMember("Paul Bureau");
        var admin = GivenAdmin();
        var adherent = await GivenAnAdherentAsync(admin);
        var id = await GivenAnnouncementAsync(author, kind: "public", published: true);
        await As(author).DeleteAsync($"{V1}/me", CancellationToken);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/announcements/{id}/comments", new { text = "Bonjour" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        _ = Api.Mail.DidNotReceive().SendAsync(Arg.Any<AccountInfo?>(), Arg.Is<string>(s => s.Contains("commentaire")), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
