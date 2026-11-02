using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class LikeAnnouncementTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsync($"{V1}/announcements/{Guid.NewGuid()}/like", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_MemberHasNotValidatedMembership()
    {
        var response = await As(GivenMember()).PostAsync($"{V1}/announcements/{Guid.NewGuid()}/like", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var response = await As(await GivenAnAdherentAsync(GivenBoardMember())).PostAsync($"{V1}/announcements/{Guid.NewGuid()}/like", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_CountTheLikeAndMarkItAsMine_When_AdherentLikesAnAnnouncement()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);

        var response = await As(adherent).PostAsync($"{V1}/announcements/{id}/like", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var announcement = await AnnouncementAsync(adherent, id);
        Assert.Equal(1, announcement.LikeCount);
        Assert.True(announcement.LikedByMe);
        var item = Assert.Single(await AnnouncementsAsync(adherent));
        Assert.Equal(1, item.LikeCount);
        Assert.True(item.LikedByMe);
    }

    [Fact]
    public async Task Should_CountOnce_When_SamePersonLikesTwice()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        await As(adherent).PostAsync($"{V1}/announcements/{id}/like", null, CancellationToken);

        var response = await As(adherent).PostAsync($"{V1}/announcements/{id}/like", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(1, (await AnnouncementAsync(adherent, id)).LikeCount);
    }

    [Fact]
    public async Task Should_CountEachPerson_When_SeveralAdherentsLike()
    {
        var board = GivenBoardMember();
        var first = await GivenAnAdherentAsync(board, "Marie Adherente");
        var second = await GivenAnAdherentAsync(board, "Julie Adherente");
        var id = await GivenAnnouncementAsync(board, published: true);
        await As(first).PostAsync($"{V1}/announcements/{id}/like", null, CancellationToken);
        await As(second).PostAsync($"{V1}/announcements/{id}/like", null, CancellationToken);

        var announcement = await AnnouncementAsync(first, id);

        Assert.Equal(2, announcement.LikeCount);
        Assert.True(announcement.LikedByMe);
    }

    [Fact]
    public async Task Should_LetTheBoardLike_When_BoardMemberLikesAnAnnouncement()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, published: true);

        var response = await As(board).PostAsync($"{V1}/announcements/{id}/like", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementIsADraft()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board);

        var response = await As(adherent).PostAsync($"{V1}/announcements/{id}/like", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
