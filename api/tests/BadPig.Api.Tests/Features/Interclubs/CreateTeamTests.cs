using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class CreateTeamTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string Url => $"{V1}/admin/interclubs/teams";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync(Url, new { name = "D4", seasonId = Guid.NewGuid() }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).PostAsJsonAsync(Url, new { name = "D4", seasonId = Guid.NewGuid() }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_NameOrSeasonIsMissing()
    {
        var board = GivenBoardMember();
        var season = await GivenTheCurrentSeasonAsync(board);

        var noName = await As(board).PostAsJsonAsync(Url, new { name = " ", seasonId = season }, CancellationToken);
        var noSeason = await As(board).PostAsJsonAsync(Url, new { name = "D4" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, noName.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, noSeason.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TheSeasonDoesNotExist()
    {
        var board = GivenBoardMember();

        var response = await As(board).PostAsJsonAsync(Url, new { name = "D4", seasonId = Guid.NewGuid() }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_TheTeamAlreadyExistsForTheSeason()
    {
        var board = GivenBoardMember();
        await GivenTeamAsync(board, "D4");
        var season = await GivenTheCurrentSeasonAsync(board);

        var response = await As(board).PostAsJsonAsync(Url, new { name = " d4 ", seasonId = season }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_CreateTheTeamWithoutCaptain_When_BoardMemberCreatesIt()
    {
        var board = GivenBoardMember();
        var season = await GivenTheCurrentSeasonAsync(board);

        var response = await As(board).PostAsJsonAsync(Url, new { name = " D4 ", seasonId = season }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var team = Assert.Single(await TeamsAsync(board));
        Assert.Equal((await ReadAsync<IdDto>(response)).Id, team.Id);
        Assert.Equal("D4", team.Name);
        Assert.Equal(season, team.SeasonId);
        Assert.Equal("2026-2027", team.Season);
        Assert.Null(team.CaptainId);
    }

    [Fact]
    public async Task Should_AcceptTheSameNameInAnotherSeason()
    {
        var board = GivenBoardMember();
        var past = await GivenSeasonAsync(board, "2025-09-01");
        await GivenTeamAsync(board, "D4", past);
        var season = await GivenTheCurrentSeasonAsync(board);

        var response = await As(board).PostAsJsonAsync(Url, new { name = "D4", seasonId = season }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
