using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class UnpinAnnouncementTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/unpin", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).PostAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/unpin", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PostAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/unpin", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_GoBackToChronologicalOrder_When_TheAnnouncementIsUnpinned()
    {
        var board = GivenBoardMember();
        var old = await GivenAnnouncementAsync(board, "Ancienne", "public", published: true, pinned: true);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 18, 9, 0, 0, TimeSpan.Zero));
        var recent = await GivenAnnouncementAsync(board, "Récente", "public", published: true);

        var response = await As(board).PostAsync($"{V1}/admin/announcements/{old}/unpin", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal([recent, old], (await AnnouncementsAsync(null)).Select(a => a.Id));
    }
}
