using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class RemoveAnnouncementAttachmentTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/attachments/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).DeleteAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/attachments/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var response = await As(GivenBoardMember()).DeleteAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/attachments/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_RemoveTheAttachment_When_ItBelongsToTheAnnouncement()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "public", published: true);
        var fileId = (await ReadAsync<IdDto>(await As(board).PostAsync($"{V1}/admin/announcements/{id}/attachments", FileForm("reglement.pdf", new byte[10]), CancellationToken))).Id;

        var response = await As(board).DeleteAsync($"{V1}/admin/announcements/{id}/attachments/{fileId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty((await AnnouncementAsync(board, id)).Files);
        Assert.Equal(HttpStatusCode.NotFound, (await Anonymous().GetAsync($"{V1}/announcements/{id}/files/{fileId}", CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AttachmentBelongsToAnotherAnnouncement()
    {
        var board = GivenBoardMember();
        var first = await GivenAnnouncementAsync(board, "Une");
        var second = await GivenAnnouncementAsync(board, "Deux");
        var fileId = (await ReadAsync<IdDto>(await As(board).PostAsync($"{V1}/admin/announcements/{first}/attachments", FileForm("reglement.pdf", new byte[10]), CancellationToken))).Id;

        var response = await As(board).DeleteAsync($"{V1}/admin/announcements/{second}/attachments/{fileId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Single((await AnnouncementAsync(board, first)).Files);
    }
}
