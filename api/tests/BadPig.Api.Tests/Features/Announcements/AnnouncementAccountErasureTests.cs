using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class AnnouncementAccountErasureTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_RemoveMyLikes_When_IDeleteMyAccount()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        await As(adherent).PostAsync($"{V1}/announcements/{id}/like", null, CancellationToken);

        await As(adherent).DeleteAsync($"{V1}/me", CancellationToken);

        Assert.Equal(0, (await AnnouncementAsync(board, id)).LikeCount);
    }

    [Fact]
    public async Task Should_KeepTheAnnouncementWithoutItsAuthorName_When_TheAuthorDeletesTheAccount()
    {
        var author = GivenBoardMember("Paul Bureau");
        var admin = GivenAdmin();
        var id = await GivenAnnouncementAsync(author, kind: "public", published: true);

        var response = await As(author).DeleteAsync($"{V1}/me", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var announcement = await AnnouncementAsync(admin, id);
        Assert.Equal("Ancien membre du bureau", announcement.AuthorName);
    }

    [Fact]
    public async Task Should_RemoveMyPollAnswers_When_IDeleteMyAccount()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenPollAsync(board, published: true);
        await GivenAnswerAsync(adherent, id, "Oui");

        await As(adherent).DeleteAsync($"{V1}/me", CancellationToken);

        Assert.Equal(0, (await AnnouncementAsync(board, id)).Poll!.ParticipantCount);
    }

    [Fact]
    public async Task Should_RemoveMyComments_When_IDeleteMyAccount()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        await GivenCommentAsync(adherent, id, "Mon commentaire");
        await GivenCommentAsync(board, id, "Celui du bureau");

        await As(adherent).DeleteAsync($"{V1}/me", CancellationToken);

        Assert.Equal("Celui du bureau", Assert.Single(await CommentsAsync(board, id)).Text);
    }
}
