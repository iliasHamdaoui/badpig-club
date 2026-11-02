using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class RemoveCaptainTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{V1}/admin/interclubs/teams/{Guid.NewGuid()}/captain", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).DeleteAsync($"{V1}/admin/interclubs/teams/{Guid.NewGuid()}/captain", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TeamDoesNotExist()
    {
        var response = await As(GivenBoardMember()).DeleteAsync($"{V1}/admin/interclubs/teams/{Guid.NewGuid()}/captain", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_LeaveTheTeamWithoutCaptain_When_BoardMemberRemovesIt()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var teamId = await GivenTeamAsync(board, captain: captain);

        var response = await As(board).DeleteAsync($"{V1}/admin/interclubs/teams/{teamId}/captain", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var team = Assert.Single(await TeamsAsync(board));
        Assert.Null(team.CaptainId);
        Assert.Null(team.CaptainName);
    }

    [Fact]
    public async Task Should_NoLongerLetThePreviousCaptainChangeFixtures_When_TheCaptainIsRemoved()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var teamId = await GivenTeamAsync(board, captain: captain);

        await As(board).DeleteAsync($"{V1}/admin/interclubs/teams/{teamId}/captain", CancellationToken);

        var response = await As(captain).PostAsJsonAsync($"{V1}/interclubs/teams/{teamId}/fixtures", new { date = "2026-11-14", opponent = "BC Lyon", venue = "home" }, CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
