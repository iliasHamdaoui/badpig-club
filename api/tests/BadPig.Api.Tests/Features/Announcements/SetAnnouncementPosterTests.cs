using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class SetAnnouncementPosterTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/poster", FileForm("a.png", PngBytes()), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).PutAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/poster", FileForm("a.png", PngBytes()), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PutAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/poster", FileForm("a.png", PngBytes()), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ShowThePosterInTheAnnouncement_When_ImageIsValid()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).PutAsync($"{V1}/admin/announcements/{id}/poster", FileForm("affiche.png", PngBytes()), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var file = Assert.Single((await AnnouncementAsync(board, id)).Files);
        Assert.True(file.IsPoster);
        Assert.Equal("image/png", file.ContentType);
        Assert.Equal("affiche.png", file.FileName);
    }

    [Fact]
    public async Task Should_ReplaceThePoster_When_AnotherImageIsSent()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);
        await As(board).PutAsync($"{V1}/admin/announcements/{id}/poster", FileForm("ancienne.png", PngBytes()), CancellationToken);
        var oldId = Assert.Single((await AnnouncementAsync(board, id)).Files).Id;

        await As(board).PutAsync($"{V1}/admin/announcements/{id}/poster", FileForm("nouvelle.png", PngBytes(128)), CancellationToken);

        var file = Assert.Single((await AnnouncementAsync(board, id)).Files);
        Assert.Equal("nouvelle.png", file.FileName);
        Assert.NotEqual(oldId, file.Id);
        Assert.Equal(HttpStatusCode.NotFound, (await As(board).GetAsync($"{V1}/announcements/{id}/files/{oldId}", CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_FileIsNotAnImage()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).PutAsync($"{V1}/admin/announcements/{id}/poster", FileForm("affiche.pdf", PngBytes()), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_ContentIsNotReallyAnImage()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).PutAsync($"{V1}/admin/announcements/{id}/poster", FileForm("affiche.png", new byte[64]), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_PosterIsTooHeavy()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).PutAsync($"{V1}/admin/announcements/{id}/poster", FileForm("affiche.png", PngBytes(5 * 1024 * 1024 + 1)), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_NoFileIsSent()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).PutAsync($"{V1}/admin/announcements/{id}/poster", new MultipartFormDataContent { { new StringContent("x"), "other" } }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
