using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class PublishAnnouncementTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/publish", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).PostAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/publish", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PostAsync($"{V1}/admin/announcements/{Guid.NewGuid()}/publish", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ShowTheAnnouncementToVisitorsWithItsPublicationDate_When_PublicAnnouncementIsPublished()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "public");
        Assert.DoesNotContain(await AnnouncementsAsync(null), a => a.Id == id);

        var response = await As(board).PostAsync($"{V1}/admin/announcements/{id}/publish", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var published = Assert.Single(await AnnouncementsAsync(null), a => a.Id == id);
        Assert.True(published.IsPublished);
        Assert.Equal(new DateTimeOffset(2026, 10, 15, 10, 0, 0, TimeSpan.Zero), published.PublishedAt);
    }

    [Fact]
    public async Task Should_KeepTheFirstPublicationDate_When_AnnouncementIsPublishedAgain()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "public", published: true);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 20, 8, 0, 0, TimeSpan.Zero));

        var response = await As(board).PostAsync($"{V1}/admin/announcements/{id}/publish", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(new DateTimeOffset(2026, 10, 15, 10, 0, 0, TimeSpan.Zero), (await AnnouncementAsync(board, id)).PublishedAt);
    }

    [Fact]
    public async Task Should_ListTheMostRecentlyPublishedFirst_When_SeveralAnnouncementsArePublishedOnDifferentDays()
    {
        var board = GivenBoardMember();
        var first = await GivenAnnouncementAsync(board, "Première", "public", published: true);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 18, 9, 0, 0, TimeSpan.Zero));
        var second = await GivenAnnouncementAsync(board, "Deuxième", "public", published: true);

        var announcements = await AnnouncementsAsync(null);

        Assert.Equal([second, first], announcements.Select(a => a.Id));
    }
}
