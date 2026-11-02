using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class ListSeasonsTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/admin/seasons";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync(Route, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).GetAsync(Route, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_FlagOnlyTheSeasonContainingToday_When_SeveralSeasonsExist()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board, startsOn: "2025-09-01");
        await GivenSeasonAsync(board, startsOn: "2026-09-01");

        var seasons = await ReadAsync<PageDto<SeasonDto>>(await As(board).GetAsync(Route, CancellationToken));

        Assert.Equal(["2026-2027"], seasons.Items.Where(s => s.IsCurrent).Select(s => s.Name));
    }

    [Fact]
    public async Task Should_ListTheMostRecentSeasonFirst_When_NoSortIsGiven()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board, startsOn: "2025-09-01");
        await GivenSeasonAsync(board, startsOn: "2026-09-01");

        var seasons = await ReadAsync<PageDto<SeasonDto>>(await As(board).GetAsync(Route, CancellationToken));

        Assert.Equal(["2026-2027", "2025-2026"], seasons.Items.Select(s => s.Name));
    }

    [Fact]
    public async Task Should_ReturnTheRequestedPage_When_PageSizeIsSmallerThanTheTotal()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board, startsOn: "2024-09-01");
        await GivenSeasonAsync(board, startsOn: "2025-09-01");
        await GivenSeasonAsync(board, startsOn: "2026-09-01");

        var page = await ReadAsync<PageDto<SeasonDto>>(await As(board).GetAsync($"{Route}?page=2&pageSize=2", CancellationToken));

        Assert.Single(page.Items);
        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.TotalPages);
    }

    [Fact]
    public async Task Should_FilterOnTheName_When_SearchIsGiven()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board, startsOn: "2025-09-01");
        await GivenSeasonAsync(board, startsOn: "2026-09-01");

        var seasons = await ReadAsync<PageDto<SeasonDto>>(await As(board).GetAsync($"{Route}?search=2025", CancellationToken));

        Assert.Equal(["2025-2026"], seasons.Items.Select(s => s.Name));
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_SortFieldIsUnknown()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{Route}?sort=unknown", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
