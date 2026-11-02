using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class DeleteAnnouncementTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string StoragePath => Api.StoragePath;

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{V1}/admin/announcements/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).DeleteAsync($"{V1}/admin/announcements/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var response = await As(GivenBoardMember()).DeleteAsync($"{V1}/admin/announcements/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_DeleteAnyAnnouncement_When_SignedInPersonIsNotTheAuthor()
    {
        var author = GivenBoardMember("Paul Bureau");
        var other = GivenBoardMember("Bruno Bureau");
        var id = await GivenAnnouncementAsync(author, kind: "public", published: true);

        var response = await As(other).DeleteAsync($"{V1}/admin/announcements/{id}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await As(other).GetAsync($"{V1}/announcements/{id}", CancellationToken)).StatusCode);
        Assert.Empty(await AnnouncementsAsync(null));
    }

    [Fact]
    public async Task Should_DeleteADraft_When_AnnouncementIsNotPublished()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).DeleteAsync($"{V1}/admin/announcements/{id}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Should_DeleteItsFiles_When_AnnouncementIsDeleted()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "public", published: true);
        await As(board).PutAsync($"{V1}/admin/announcements/{id}/poster", FileForm("affiche.png", PngBytes()), CancellationToken);
        var posterId = (await AnnouncementAsync(board, id)).Files.Single().Id;

        await As(board).DeleteAsync($"{V1}/admin/announcements/{id}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, (await As(board).GetAsync($"{V1}/announcements/{id}/files/{posterId}", CancellationToken)).StatusCode);
        Assert.Empty(Directory.Exists(StoragePath) ? Directory.GetFiles(StoragePath, "*", SearchOption.AllDirectories) : []);
    }

    [Fact]
    public async Task Should_DeleteTheLikes_When_AnnouncementIsDeleted()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, published: true);
        await As(adherent).PostAsync($"{V1}/announcements/{id}/like", null, CancellationToken);

        var response = await As(board).DeleteAsync($"{V1}/admin/announcements/{id}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await As(adherent).PostAsync($"{V1}/announcements/{id}/like", null, CancellationToken)).StatusCode);
    }
}
