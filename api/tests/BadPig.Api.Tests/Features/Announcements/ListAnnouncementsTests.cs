using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class ListAnnouncementsTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnOnlyPublishedPublicAnnouncements_When_NobodyIsSignedIn()
    {
        var board = GivenBoardMember();
        var visible = await GivenAnnouncementAsync(board, "Visible", "public", published: true);
        await GivenAnnouncementAsync(board, "Brouillon public", "public");
        await GivenAnnouncementAsync(board, "Interne", "internal", published: true);

        var announcements = await AnnouncementsAsync(null);

        Assert.Equal([visible], announcements.Select(a => a.Id));
        Assert.All(announcements, a => Assert.Null(a.AuthorName));
        Assert.All(announcements, a => Assert.Null(a.LikeCount));
    }

    [Fact]
    public async Task Should_ReturnPublishedInternalAndPublicAnnouncements_When_PersonIsAnAdherent()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var publicOne = await GivenAnnouncementAsync(board, "Publique", "public", published: true);
        var internalOne = await GivenAnnouncementAsync(board, "Interne", "internal", published: true);
        await GivenAnnouncementAsync(board, "Brouillon", "internal");

        var announcements = await AnnouncementsAsync(adherent);

        Assert.Equivalent(new[] { publicOne, internalOne }, announcements.Select(a => a.Id));
        Assert.All(announcements, a => Assert.Null(a.AuthorName));
    }

    [Fact]
    public async Task Should_ReturnPublicAnnouncementsOnly_When_MemberHasNotValidatedMembership()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        var publicOne = await GivenAnnouncementAsync(board, "Publique", "public", published: true);
        await GivenAnnouncementAsync(board, "Interne", "internal", published: true);

        var announcements = await AnnouncementsAsync(member);

        Assert.Equal([publicOne], announcements.Select(a => a.Id));
    }

    [Fact]
    public async Task Should_ReturnDraftsAndAuthors_When_PersonIsOnTheBoard()
    {
        var board = GivenBoardMember("Paul Bureau");
        await GivenAnnouncementAsync(board, "Brouillon", "internal");
        await GivenAnnouncementAsync(board, "Publiée", "public", published: true);

        var announcements = await AnnouncementsAsync(board);

        Assert.Equal(2, announcements.Count);
        Assert.All(announcements, a => Assert.Equal("Paul Bureau", a.AuthorName));
    }

    [Fact]
    public async Task Should_ListTheMostRecentFirst_When_NoSortIsGiven()
    {
        var board = GivenBoardMember();
        var first = await GivenAnnouncementAsync(board, "Première", "public", published: true);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 18, 9, 0, 0, TimeSpan.Zero));
        var second = await GivenAnnouncementAsync(board, "Deuxième", "public", published: true);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 20, 9, 0, 0, TimeSpan.Zero));
        var third = await GivenAnnouncementAsync(board, "Troisième", "public", published: true);

        Assert.Equal([third, second, first], (await AnnouncementsAsync(null)).Select(a => a.Id));
    }

    [Fact]
    public async Task Should_PutPinnedAnnouncementsFirst_When_SomeArePinned()
    {
        var board = GivenBoardMember();
        var pinned = await GivenAnnouncementAsync(board, "Épinglée", "public", published: true, pinned: true);
        Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 18, 9, 0, 0, TimeSpan.Zero));
        var recent = await GivenAnnouncementAsync(board, "Récente", "public", published: true);

        Assert.Equal([pinned, recent], (await AnnouncementsAsync(null)).Select(a => a.Id));
    }

    [Fact]
    public async Task Should_FilterByKindAndStatus_When_QueryIsGiven()
    {
        var board = GivenBoardMember();
        var publicOne = await GivenAnnouncementAsync(board, "Publique", "public", published: true);
        var draft = await GivenAnnouncementAsync(board, "Brouillon", "internal");
        await GivenAnnouncementAsync(board, "Interne", "internal", published: true);

        Assert.Equal([publicOne], (await AnnouncementsAsync(board, "?kind=public")).Select(a => a.Id));
        Assert.Equal([draft], (await AnnouncementsAsync(board, "?status=draft")).Select(a => a.Id));
        Assert.Equal(2, (await AnnouncementsAsync(board, "?status=published")).Count);
    }

    [Fact]
    public async Task Should_SearchInTitleAndSummary_When_SearchIsGiven()
    {
        var board = GivenBoardMember();
        var match = await GivenAnnouncementAsync(board, "Tournoi de Noël", "public", published: true);
        await GivenAnnouncementAsync(board, "Assemblée", "public", published: true, bodyHtml: "<p>Ordre du jour</p>");
        var inSummary = await GivenAnnouncementAsync(board, "Autre", "public", published: true, bodyHtml: "<p>Venez au tournoi</p>");

        var announcements = await AnnouncementsAsync(null, "?search=tournoi");

        Assert.Equivalent(new[] { match, inSummary }, announcements.Select(a => a.Id));
    }

    [Fact]
    public async Task Should_ReturnASummaryWithoutMarkup_When_BodyHasRichText()
    {
        var board = GivenBoardMember();
        await GivenAnnouncementAsync(board, "Riche", "public", published: true, bodyHtml: "<p><strong>Bonjour</strong> à tous</p>");

        var item = Assert.Single(await AnnouncementsAsync(null));

        Assert.Equal("Bonjour à tous", item.Summary);
    }

    [Fact]
    public async Task Should_ExposeThePosterAndAttachmentCount_When_AnnouncementHasFiles()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "public", published: true);
        await As(board).PutAsync($"{V1}/admin/announcements/{id}/poster", FileForm("affiche.png", PngBytes()), CancellationToken);
        await As(board).PostAsync($"{V1}/admin/announcements/{id}/attachments", FileForm("a.pdf", new byte[10]), CancellationToken);
        await As(board).PostAsync($"{V1}/admin/announcements/{id}/attachments", FileForm("b.pdf", new byte[10]), CancellationToken);

        var item = Assert.Single(await AnnouncementsAsync(null));

        Assert.NotNull(item.PosterId);
        Assert.Equal(2, item.AttachmentCount);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_SortIsUnknown()
    {
        var response = await Anonymous().GetAsync($"{V1}/announcements?sort=likes", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnOnePage_When_PageSizeIsGiven()
    {
        var board = GivenBoardMember();
        for (var i = 0; i < 3; i++)
            await GivenAnnouncementAsync(board, $"Annonce {i}", "public", published: true);

        var page = await ReadAsync<PageDto<AnnouncementItemDto>>(await Anonymous().GetAsync($"{V1}/announcements?pageSize=2&page=2", CancellationToken));

        Assert.Single(page.Items);
        Assert.Equal(3, page.TotalCount);
    }

    [Fact]
    public async Task Should_ReturnAFormattedPreviewWithoutLinks_When_TheTextHasRichFormatting()
    {
        var board = GivenBoardMember();
        await GivenAnnouncementAsync(board, "Tournoi", "public", published: true,
            bodyHtml: "<h2>Quand ?</h2><ul><li>Le 25 novembre</li></ul><p>Infos sur <a href=\"https://exemple.fr\">Badnet</a></p>");

        var item = Assert.Single(await AnnouncementsAsync(null));

        Assert.Contains("<h2>Quand ?</h2>", item.PreviewHtml);
        Assert.Contains("<li>Le 25 novembre</li>", item.PreviewHtml);
        Assert.Contains("Badnet", item.PreviewHtml);
        Assert.DoesNotContain("<a", item.PreviewHtml);
        Assert.False(item.HasMore);
    }

    [Fact]
    public async Task Should_CutThePreviewBetweenTwoBlocksAndFlagIt_When_TheTextIsLong()
    {
        var board = GivenBoardMember();
        await GivenAnnouncementAsync(board, "Long", "public", published: true,
            bodyHtml: $"<p>{new string('A', 300)}</p><p>{new string('B', 300)}</p><p>{new string('C', 300)}</p>");

        var item = Assert.Single(await AnnouncementsAsync(null));

        Assert.Contains(new string('A', 300), item.PreviewHtml);
        Assert.DoesNotContain("B", item.PreviewHtml);
        Assert.True(item.HasMore);
    }
}
