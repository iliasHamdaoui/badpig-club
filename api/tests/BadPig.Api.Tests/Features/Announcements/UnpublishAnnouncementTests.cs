using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class UnpublishAnnouncementTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/unpublish", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).PostAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/unpublish", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PostAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/unpublish", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_HideTheAnnouncementFromVisitors_When_ItIsUnpublished()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "public", published: true);

        var response = await As(board).PostAsync($"{V1}/admin/announcements/{id}/unpublish", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await AnnouncementsAsync(null));
        var draft = await AnnouncementAsync(board, id);
        Assert.False(draft.IsPublished);
        Assert.Null(draft.PublishedAt);
    }

    [Fact]
    public async Task Should_ReturnNoContent_When_AnnouncementIsAlreadyADraft()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).PostAsync($"{V1}/admin/announcements/{id}/unpublish", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
