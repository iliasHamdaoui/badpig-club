using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class ListTitlesTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/admin/titles";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync(Route, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).GetAsync(Route, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ShowTheHolder_When_TitleIsGivenToABoardMember()
    {
        var board = GivenBoardMember("Paul Bureau");
        var titleId = await GivenTitleAsync(board, "Trésorier(e)");
        await As(board).PutAsJsonAsync($"{V1}/admin/users/{board.Id}/title", new { titleId }, CancellationToken);

        var titles = await ReadAsync<PageDto<TitleDto>>(await As(board).GetAsync(Route, CancellationToken));

        var title = Assert.Single(titles.Items);
        Assert.Equal(board.Id, title.HolderId);
        Assert.Equal("Paul Bureau", title.HolderName);
    }

    [Fact]
    public async Task Should_FilterOnTheLabel_When_SearchIsGiven()
    {
        var board = GivenBoardMember();
        await GivenTitleAsync(board, "Secrétaire");
        await GivenTitleAsync(board, "Trésorier(e)");

        var titles = await ReadAsync<PageDto<TitleDto>>(await As(board).GetAsync($"{Route}?search=secr", CancellationToken));

        Assert.Equal(["Secrétaire"], titles.Items.Select(t => t.Label));
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_SortFieldIsUnknown()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{Route}?sort=unknown", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
