using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class GetAnnouncementFileTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ServeThePosterInline_When_AnnouncementIsPublic()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "public", published: true);
        await As(board).PutAsync($"{V1}/admin/announcements/{id}/poster", FileForm("affiche.png", PngBytes(100)), CancellationToken);
        var posterId = Assert.Single((await AnnouncementAsync(null, id)).Files).Id;

        var response = await Anonymous().GetAsync($"{V1}/announcements/{id}/files/{posterId}", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        Assert.NotEqual("attachment", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal(100, (await response.Content.ReadAsByteArrayAsync(CancellationToken)).Length);
    }

    [Fact]
    public async Task Should_ServeTheAttachmentAsADownload_When_AnnouncementIsPublic()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "public", published: true);
        var fileId = (await ReadAsync<IdDto>(await As(board).PostAsync($"{V1}/admin/announcements/{id}/attachments", FileForm("reglement.pdf", new byte[30]), CancellationToken))).Id;

        var response = await Anonymous().GetAsync($"{V1}/announcements/{id}/files/{fileId}", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("attachment", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal("reglement.pdf", response.Content.Headers.ContentDisposition?.FileName);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_VisitorAsksForAFileOfAnInternalAnnouncement()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "internal", published: true);
        var fileId = (await ReadAsync<IdDto>(await As(board).PostAsync($"{V1}/admin/announcements/{id}/attachments", FileForm("reglement.pdf", new byte[30]), CancellationToken))).Id;

        var response = await Anonymous().GetAsync($"{V1}/announcements/{id}/files/{fileId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnTheFile_When_AdherentAsksForAFileOfAnInternalAnnouncement()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, kind: "internal", published: true);
        var fileId = (await ReadAsync<IdDto>(await As(board).PostAsync($"{V1}/admin/announcements/{id}/attachments", FileForm("reglement.pdf", new byte[30]), CancellationToken))).Id;

        var response = await As(adherent).GetAsync($"{V1}/announcements/{id}/files/{fileId}", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_FileBelongsToAnotherAnnouncement()
    {
        var board = GivenBoardMember();
        var first = await GivenAnnouncementAsync(board, "Une", "public", published: true);
        var second = await GivenAnnouncementAsync(board, "Deux", "public", published: true);
        var fileId = (await ReadAsync<IdDto>(await As(board).PostAsync($"{V1}/admin/announcements/{first}/attachments", FileForm("reglement.pdf", new byte[30]), CancellationToken))).Id;

        var response = await Anonymous().GetAsync($"{V1}/announcements/{second}/files/{fileId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
