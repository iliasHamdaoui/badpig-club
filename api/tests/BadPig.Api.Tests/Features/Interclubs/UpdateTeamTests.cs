using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class UpdateTeamTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync($"{V1}/admin/interclubs/teams/{Guid.NewGuid()}", new { name = "D5", seasonId = Guid.NewGuid() }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).PutAsJsonAsync($"{V1}/admin/interclubs/teams/{Guid.NewGuid()}", new { name = "D5", seasonId = Guid.NewGuid() }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TeamDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PutAsJsonAsync($"{V1}/admin/interclubs/teams/{Guid.NewGuid()}", new { name = "D5", seasonId = Guid.NewGuid() }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_NameIsMissing()
    {
        var board = GivenBoardMember();
        var id = await GivenTeamAsync(board);
        var season = await GivenTheCurrentSeasonAsync(board);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/interclubs/teams/{id}", new { name = "", seasonId = season }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TheSeasonDoesNotExist()
    {
        var board = GivenBoardMember();
        var id = await GivenTeamAsync(board);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/interclubs/teams/{id}", new { name = "D5", seasonId = Guid.NewGuid() }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_AnotherTeamHasTheSameNameInTheSeason()
    {
        var board = GivenBoardMember();
        await GivenTeamAsync(board, "D4");
        var id = await GivenTeamAsync(board, "D5");
        var season = await GivenTheCurrentSeasonAsync(board);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/interclubs/teams/{id}", new { name = "D4", seasonId = season }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_ChangeTheTeamAndKeepItsCaptain_When_BoardMemberChangesIt()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var id = await GivenTeamAsync(board, "D4", captain: captain);
        var next = await GivenSeasonAsync(board, "2027-09-01");

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/interclubs/teams/{id}", new { name = "D3", seasonId = next }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var team = Assert.Single(await TeamsAsync(board));
        Assert.Equal("D3", team.Name);
        Assert.Equal(next, team.SeasonId);
        Assert.Equal("2027-2028", team.Season);
        Assert.Equal(captain.Id, team.CaptainId);
    }

    [Fact]
    public async Task Should_AcceptTheTeamOwnValues_When_NothingChanges()
    {
        var board = GivenBoardMember();
        var id = await GivenTeamAsync(board, "D4");
        var season = await GivenTheCurrentSeasonAsync(board);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/interclubs/teams/{id}", new { name = "D4", seasonId = season }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
