using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class RemoveAnnouncementPosterTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/poster", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).DeleteAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/poster", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var response = await As(GivenBoardMember()).DeleteAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/poster", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_RemoveThePoster_When_AnnouncementHasOne()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);
        await As(board).PutAsync($"{V1}/admin/announcements/{id}/poster", FileForm("affiche.png", PngBytes()), CancellationToken);
        var posterId = Assert.Single((await AnnouncementAsync(board, id)).Files).Id;

        var response = await As(board).DeleteAsync($"{V1}/admin/announcements/{id}/poster", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty((await AnnouncementAsync(board, id)).Files);
        Assert.Equal(HttpStatusCode.NotFound, (await As(board).GetAsync($"{V1}/announcements/{id}/files/{posterId}", CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNoContent_When_AnnouncementHasNoPoster()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).DeleteAsync($"{V1}/admin/announcements/{id}/poster", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
