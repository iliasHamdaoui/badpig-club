using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class UnlikeAnnouncementTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{V1}/announcements/{Guid.NewGuid()}/like", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_MemberHasNotValidatedMembership()
    {
        var response = await As(GivenMember()).DeleteAsync($"{V1}/announcements/{Guid.NewGuid()}/like", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var response = await As(await GivenAnAdherentAsync(GivenBoardMember())).DeleteAsync($"{V1}/announcements/{Guid.NewGuid()}/like", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_RemoveMyLike_When_IUnlikeAnAnnouncement()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        await As(adherent).PostAsync($"{V1}/announcements/{id}/like", null, CancellationToken);

        var response = await As(adherent).DeleteAsync($"{V1}/announcements/{id}/like", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var announcement = await AnnouncementAsync(adherent, id);
        Assert.Equal(0, announcement.LikeCount);
        Assert.False(announcement.LikedByMe);
    }

    [Fact]
    public async Task Should_KeepOtherLikes_When_OnePersonUnlikes()
    {
        var board = GivenBoardMember();
        var first = await GivenAnAdherentAsync(board, "Marie Adherente");
        var second = await GivenAnAdherentAsync(board, "Julie Adherente");
        var id = await GivenAnnouncementAsync(board, published: true);
        await As(first).PostAsync($"{V1}/announcements/{id}/like", null, CancellationToken);
        await As(second).PostAsync($"{V1}/announcements/{id}/like", null, CancellationToken);

        await As(first).DeleteAsync($"{V1}/announcements/{id}/like", CancellationToken);

        Assert.Equal(1, (await AnnouncementAsync(second, id)).LikeCount);
    }

    [Fact]
    public async Task Should_ReturnNoContent_When_IHadNotLikedTheAnnouncement()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);

        var response = await As(adherent).DeleteAsync($"{V1}/announcements/{id}/like", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
