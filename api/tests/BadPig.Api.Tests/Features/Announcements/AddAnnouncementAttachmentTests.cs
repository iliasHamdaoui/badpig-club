using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class AddAnnouncementAttachmentTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/attachments", FileForm("r.pdf", new byte[10]), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).PostAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/attachments", FileForm("r.pdf", new byte[10]), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PostAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/attachments", FileForm("r.pdf", new byte[10]), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnCreatedWithTheId_When_FileIsAllowed()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).PostAsync($"{V1}/admin/announcements/{id}/attachments", FileForm("reglement.pdf", new byte[2048]), CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await ReadAsync<IdDto>(response);
        var file = Assert.Single((await AnnouncementAsync(board, id)).Files);
        Assert.Equal(created.Id, file.Id);
        Assert.False(file.IsPoster);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal(2048, file.SizeBytes);
    }

    [Fact]
    public async Task Should_DecideTheTypeFromTheExtension_When_BrowserSendsAnotherType()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        await As(board).PostAsync($"{V1}/admin/announcements/{id}/attachments", FileForm("reglement.pdf", new byte[10], "text/html"), CancellationToken);

        Assert.Equal("application/pdf", Assert.Single((await AnnouncementAsync(board, id)).Files).ContentType);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_ExtensionIsNotAllowed()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).PostAsync($"{V1}/admin/announcements/{id}/attachments", FileForm("virus.exe", new byte[10]), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_FileIsEmpty()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).PostAsync($"{V1}/admin/announcements/{id}/attachments", FileForm("vide.pdf", []), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_FileIsTooHeavy()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var response = await As(board).PostAsync($"{V1}/admin/announcements/{id}/attachments", FileForm("gros.pdf", new byte[10 * 1024 * 1024 + 1]), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_AnnouncementAlreadyHasTenAttachments()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);
        for (var i = 0; i < 10; i++)
            Assert.Equal(HttpStatusCode.Created, (await As(board).PostAsync($"{V1}/admin/announcements/{id}/attachments", FileForm($"doc{i}.pdf", new byte[10]), CancellationToken)).StatusCode);

        var response = await As(board).PostAsync($"{V1}/admin/announcements/{id}/attachments", FileForm("onze.pdf", new byte[10]), CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
