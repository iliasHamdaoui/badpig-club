using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Announcements;

public sealed class GetAnnouncementTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnThePublicAnnouncement_When_NobodyIsSignedIn()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, "Portes ouvertes", "public", published: true);

        var announcement = await AnnouncementAsync(null, id);

        Assert.Equal("Portes ouvertes", announcement.Title);
        Assert.Null(announcement.AuthorName);
        Assert.Null(announcement.LikeCount);
        Assert.False(announcement.LikedByMe);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_VisitorAsksForAnInternalAnnouncement()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "internal", published: true);

        var response = await Anonymous().GetAsync($"{V1}/announcements/{id}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_VisitorAsksForADraft()
    {
        var board = GivenBoardMember();
        var id = await GivenAnnouncementAsync(board, kind: "public");

        var response = await Anonymous().GetAsync($"{V1}/announcements/{id}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AdherentAsksForADraft()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, kind: "public");

        var response = await As(adherent).GetAsync($"{V1}/announcements/{id}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnTheInternalAnnouncement_When_PersonIsAnAdherent()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenAnnouncementAsync(board, kind: "internal", published: true);

        var announcement = await AnnouncementAsync(adherent, id);

        Assert.Equal(0, announcement.LikeCount);
        Assert.Null(announcement.AuthorName);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_MemberHasNotValidatedMembershipAndAnnouncementIsInternal()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        var id = await GivenAnnouncementAsync(board, kind: "internal", published: true);

        var response = await As(member).GetAsync($"{V1}/announcements/{id}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnThePublicAnnouncement_When_MemberHasNotValidatedMembership()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        var id = await GivenAnnouncementAsync(board, kind: "public", published: true);

        var announcement = await AnnouncementAsync(member, id);

        Assert.Null(announcement.LikeCount);
    }

    [Fact]
    public async Task Should_ShowDraftsAndTheAuthor_When_PersonIsOnTheBoard()
    {
        var author = GivenBoardMember("Paul Bureau");
        var reader = GivenBoardMember("Bruno Bureau");
        var id = await GivenAnnouncementAsync(author);

        var announcement = await AnnouncementAsync(reader, id);

        Assert.Equal("Paul Bureau", announcement.AuthorName);
        Assert.False(announcement.IsPublished);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AnnouncementDoesNotExist()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{V1}/announcements/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_IdentifierIsNotAGuid()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{V1}/announcements/abc", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
