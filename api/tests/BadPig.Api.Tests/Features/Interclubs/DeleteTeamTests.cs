using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class DeleteTeamTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{V1}/admin/interclubs/teams/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).DeleteAsync($"{V1}/admin/interclubs/teams/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TeamDoesNotExist()
    {
        var response = await As(GivenBoardMember()).DeleteAsync($"{V1}/admin/interclubs/teams/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_DeleteTheTeamAndItsFixtures_When_BoardMemberDeletesIt()
    {
        var board = GivenBoardMember();
        var keptTeam = await GivenTeamAsync(board, "D5");
        var keptFixture = await GivenFixtureAsync(board, keptTeam);
        var id = await GivenTeamAsync(board, "D4");
        await GivenFixtureAsync(board, id);

        var response = await As(board).DeleteAsync($"{V1}/admin/interclubs/teams/{id}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("D5", Assert.Single(await TeamsAsync(board)).Name);
        Assert.Equal(keptFixture, Assert.Single(await FixturesAsync(board)).Id);
    }
}
