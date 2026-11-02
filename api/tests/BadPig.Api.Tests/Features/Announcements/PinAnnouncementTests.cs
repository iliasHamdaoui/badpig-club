using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class PinAnnouncementTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/pin", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).PostAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/pin", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PostAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/pin", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ListThePinnedAnnouncementFirst_When_ItIsOlderThanTheOthers()
    {
        var board = GivenBoardMember();
        var old = await GivenAnnouncementAsync(board, "Ancienne", "public", published: true);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 18, 9, 0, 0, TimeSpan.Zero));
        var recent = await GivenAnnouncementAsync(board, "Récente", "public", published: true);

        var response = await As(board).PostAsync($"{V1}/admin/announcements/{old}/pin", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var announcements = await AnnouncementsAsync(null);
        Assert.Equal([old, recent], announcements.Select(a => a.Id));
        Assert.True(announcements[0].IsPinned);
    }
}
