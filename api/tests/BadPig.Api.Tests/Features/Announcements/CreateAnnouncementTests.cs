using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class CreateAnnouncementTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/admin/announcements";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync(Route, new { title = "Tournoi", bodyHtml = "<p>Texte</p>", kind = "internal" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).PostAsJsonAsync(Route, new { title = "Tournoi", bodyHtml = "<p>Texte</p>", kind = "internal" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnCreatedWithTheIdOnly_When_DataIsValid()
    {
        var board = GivenBoardMember();

        var response = await As(board).PostAsJsonAsync(Route, new { title = "Tournoi", bodyHtml = "<p>Texte</p>", kind = "internal" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(CancellationToken);
        Assert.Equal(["id"], body.EnumerateObject().Select(p => p.Name));
    }

    [Fact]
    public async Task Should_CreateADraftWithTheAuthorVisibleToTheBoard_When_AnnouncementIsCreated()
    {
        var board = GivenBoardMember("Paul Bureau");
        var id = await GivenAnnouncementAsync(board);

        var announcement = await AnnouncementAsync(board, id);

        Assert.False(announcement.IsPublished);
        Assert.False(announcement.IsPinned);
        Assert.Null(announcement.PublishedAt);
        Assert.Equal("Paul Bureau", announcement.AuthorName);
    }

    [Fact]
    public async Task Should_RemoveScripts_When_ContentContainsUnsafeMarkup()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, bodyHtml: "<p onclick=\"steal()\">Bonjour</p><script>alert(1)</script>");

        var announcement = await AnnouncementAsync(board, id);

        Assert.Contains("Bonjour", announcement.BodyHtml);
        Assert.DoesNotContain("script", announcement.BodyHtml);
        Assert.DoesNotContain("onclick", announcement.BodyHtml);
    }

    [Fact]
    public async Task Should_KeepRichText_When_ContentUsesAllowedMarkup()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, bodyHtml: "<h2>Titre</h2><p><strong>Gras</strong> et <em>italique</em> 🏸</p><ul><li>Un</li></ul><a href=\"https://example.test/page\">lien</a>");

        var announcement = await AnnouncementAsync(board, id);

        Assert.Contains("<strong>Gras</strong>", announcement.BodyHtml);
        Assert.Contains("<li>Un</li>", announcement.BodyHtml);
        Assert.Contains("🏸", announcement.BodyHtml);
        Assert.Contains("href=\"https://example.test/page\"", announcement.BodyHtml);
    }

    [Fact]
    public async Task Should_RemoveTheLink_When_LinkUsesAnUnsafeScheme()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, bodyHtml: "<p><a href=\"javascript:alert(1)\">cliquez</a></p>");

        var announcement = await AnnouncementAsync(board, id);

        Assert.DoesNotContain("javascript", announcement.BodyHtml);
    }

    [Fact]
    public async Task Should_ReturnBadRequestOnTheContentField_When_NothingRemainsAfterCleaning()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, new { title = "Tournoi", bodyHtml = "<script>alert(1)</script>", kind = "internal" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(CancellationToken)).GetProperty("errors");
        Assert.True(errors.TryGetProperty("bodyHtml", out _));
    }

    [Fact]
    public async Task Should_ReturnBadRequestOnEachInvalidField_When_TitleContentAndKindAreMissing()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, new { }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(CancellationToken)).GetProperty("errors");
        Assert.True(errors.TryGetProperty("title", out _));
        Assert.True(errors.TryGetProperty("bodyHtml", out _));
        Assert.True(errors.TryGetProperty("kind", out _));
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_KindIsUnknown()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, new { title = "Tournoi", bodyHtml = "<p>Texte</p>", kind = "secret" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TitleIsTooLong()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, new { title = new string('x', 151), bodyHtml = "<p>Texte</p>", kind = "public" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_EndNinetyDaysFromToday_When_NoEndDateIsGiven()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board);

        var announcement = await AnnouncementAsync(board, id);

        Assert.Equal(new DateOnly(2027, 1, 13), announcement.ExpiresOn);
        Assert.False(announcement.IsExpired);
    }

    [Fact]
    public async Task Should_KeepTheEndDate_When_ItIsGiven()
    {
        var board = GivenBoardMember();

        var response = await As(board).PostAsJsonAsync(Route, new { title = "Tournoi", bodyHtml = "<p>Texte</p>", kind = "public", expiresOn = "2026-12-01" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(new DateOnly(2026, 12, 1), (await AnnouncementAsync(board, (await ReadAsync<IdDto>(response)).Id)).ExpiresOn);
    }

    [Fact]
    public async Task Should_AcceptToday_When_EndDateIsToday()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, new { title = "Tournoi", bodyHtml = "<p>Texte</p>", kind = "public", expiresOn = "2026-10-15" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequestOnTheEndDate_When_ItIsInThePast()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, new { title = "Tournoi", bodyHtml = "<p>Texte</p>", kind = "public", expiresOn = "2026-10-14" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(CancellationToken)).GetProperty("errors");
        Assert.True(errors.TryGetProperty("expiresOn", out _));
    }

    [Fact]
    public async Task Should_KeepTheTextAndLinks_When_ContentUsesDivBlocks()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, bodyHtml: "Première ligne<div>Deuxième ligne</div><div><a href=\"https://youtu.be/dQw4w9WgXcQ\">https://youtu.be/dQw4w9WgXcQ</a></div>");

        var announcement = await AnnouncementAsync(board, id);

        Assert.Contains("Première ligne", announcement.BodyHtml);
        Assert.Contains("Deuxième ligne", announcement.BodyHtml);
        Assert.Contains("href=\"https://youtu.be/dQw4w9WgXcQ\"", announcement.BodyHtml);
    }

    [Fact]
    public async Task Should_OpenTheComments_When_NoChoiceIsGiven()
    {
        var board = GivenBoardMember();

        var id = await GivenAnnouncementAsync(board, published: true);

        Assert.True((await AnnouncementAsync(board, id)).CommentsEnabled);
    }

    [Fact]
    public async Task Should_CloseTheComments_When_AuthorTurnsThemOff()
    {
        var board = GivenBoardMember();

        var id = await GivenAnnouncementAsync(board, published: true, commentsEnabled: false);

        Assert.False((await AnnouncementAsync(board, id)).CommentsEnabled);
    }
}
