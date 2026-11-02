using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class AnnouncementExpiryTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static readonly DateTimeOffset LastDay = new(2027, 1, 13, 11, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DayAfter = new(2027, 1, 14, 11, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Should_StillShowTheAnnouncement_When_TodayIsTheEndDate()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "public", published: true);
        Api.Clock.UtcNow.Returns(LastDay);

        Assert.Contains(await AnnouncementsAsync(null), a => a.Id == id);
    }

    [Fact]
    public async Task Should_HideTheAnnouncementFromVisitors_When_EndDateHasPassed()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "public", published: true);
        Api.Clock.UtcNow.Returns(DayAfter);

        Assert.Empty(await AnnouncementsAsync(null));
        Assert.Equal(HttpStatusCode.NotFound, (await Anonymous().GetAsync($"{V1}/announcements/{id}", CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_HideTheAnnouncementFromAdherents_When_EndDateHasPassed()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, kind: "internal", published: true);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 12, 20, 9, 0, 0, TimeSpan.Zero));
        await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", new { title = "Titre", bodyHtml = "<p>Texte</p>", kind = "internal", expiresOn = "2026-12-20" }, CancellationToken);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 12, 21, 9, 0, 0, TimeSpan.Zero));

        Assert.Empty(await AnnouncementsAsync(adherent));
        Assert.Equal(HttpStatusCode.NotFound, (await As(adherent).GetAsync($"{V1}/announcements/{id}", CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFoundOnLikeAndFile_When_EndDateHasPassed()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, kind: "internal", published: true);
        var fileId = (await ReadAsync<IdDto>(await As(board).PostAsync($"{V1}/admin/announcements/{id}/attachments", FileForm("reglement.pdf", new byte[10]), CancellationToken))).Id;
        Api.Clock.UtcNow.Returns(DayAfter);

        Assert.Equal(HttpStatusCode.NotFound, (await As(adherent).PostAsync($"{V1}/announcements/{id}/like", null, CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await As(adherent).GetAsync($"{V1}/announcements/{id}/files/{fileId}", CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_ShowTheExpiredAnnouncementToTheBoardAsExpired_When_EndDateHasPassed()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "public", published: true);
        Api.Clock.UtcNow.Returns(DayAfter);

        var announcement = await AnnouncementAsync(board, id);
        var item = Assert.Single(await AnnouncementsAsync(board));

        Assert.True(announcement.IsExpired);
        Assert.True(item.IsExpired);
    }

    [Fact]
    public async Task Should_ShowTheAnnouncementAgain_When_TheBoardExtendsTheEndDate()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "public", published: true);
        Api.Clock.UtcNow.Returns(DayAfter);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/announcements/{id}", new { title = "Titre", bodyHtml = "<p>Texte</p>", kind = "public", expiresOn = "2027-03-01" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Contains(await AnnouncementsAsync(null), a => a.Id == id);
    }

    [Fact]
    public async Task Should_ExposeTheEndDateInTheList_When_AnnouncementsAreListed()
    {
        var board = GivenBoardMember();
        await GivenAnnouncementAsync(board, kind: "public", published: true);

        var item = Assert.Single(await AnnouncementsAsync(null));

        Assert.Equal(new DateOnly(2027, 1, 13), item.ExpiresOn);
        Assert.False(item.IsExpired);
    }
}
