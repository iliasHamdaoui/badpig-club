using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Usage;

public sealed class RecordPageVisitTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Url = $"{V1}/usage/visits";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync(Url, new { path = "/club", views = 1, seconds = 5 }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_RecordTheVisit_When_AMemberReportsAPage()
    {
        var board = GivenBoardMember();

        var response = await As(GivenMember()).PostAsJsonAsync(Url, new { path = "/announcements/:id", views = 1, seconds = 42 }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var item = Assert.Single(await UsageAsync(board));
        Assert.Equal("/announcements/:id", item.Path);
        Assert.Equal(1, item.Views);
        Assert.Equal(42, item.TotalSeconds);
    }

    [Fact]
    public async Task Should_AddUpTheVisits_When_SeveralPeopleOpenTheSamePage()
    {
        var board = GivenBoardMember();
        await GivenPageVisitAsync(GivenMember("Marie Membre"), "/club", 30);
        await GivenPageVisitAsync(GivenMember("Jean Membre"), "/club", 10);
        await GivenPageVisitAsync(GivenMember("Jean Membre"), "/club", 20, views: 0);

        var item = Assert.Single(await UsageAsync(board));

        Assert.Equal(2, item.Views);
        Assert.Equal(60, item.TotalSeconds);
        Assert.Equal(30, item.AverageSeconds);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_ThePageIsNotARouteTemplate()
    {
        var member = GivenMember();

        var withIdentifier = await As(member).PostAsJsonAsync(Url, new { path = "/announcements/Marie?x=1", views = 1, seconds = 5 }, CancellationToken);
        var withoutSlash = await As(member).PostAsJsonAsync(Url, new { path = "club", views = 1, seconds = 5 }, CancellationToken);
        var empty = await As(member).PostAsJsonAsync(Url, new { path = "", views = 1, seconds = 5 }, CancellationToken);
        var tooLong = await As(member).PostAsJsonAsync(Url, new { path = "/" + new string('a', 120), views = 1, seconds = 5 }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, withIdentifier.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, withoutSlash.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, empty.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheViewsOrTheDurationAreOutOfRange()
    {
        var member = GivenMember();

        var manyViews = await As(member).PostAsJsonAsync(Url, new { path = "/club", views = 2, seconds = 5 }, CancellationToken);
        var negative = await As(member).PostAsJsonAsync(Url, new { path = "/club", views = 1, seconds = -1 }, CancellationToken);
        var tooLong = await As(member).PostAsJsonAsync(Url, new { path = "/club", views = 1, seconds = 30 * 60 + 1 }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, manyViews.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, negative.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);
    }
}
