using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class DeleteFixtureTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{V1}/interclubs/fixtures/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_FixtureDoesNotExist()
    {
        var response = await As(GivenBoardMember()).DeleteAsync($"{V1}/interclubs/fixtures/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotTheCaptainOfTheTeam()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenFixtureAsync(board, await GivenTeamAsync(board));

        var response = await As(adherent).DeleteAsync($"{V1}/interclubs/fixtures/{id}", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Single(await FixturesAsync(board));
    }

    [Fact]
    public async Task Should_DeleteTheFixture_When_TheCaptainDeletesIt()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var teamId = await GivenTeamAsync(board, captain: captain);
        var id = await GivenFixtureAsync(captain, teamId);
        var kept = await GivenFixtureAsync(captain, teamId, opponent: "BC Nantes");

        var response = await As(captain).DeleteAsync($"{V1}/interclubs/fixtures/{id}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(kept, Assert.Single(await FixturesAsync(captain)).Id);
    }
}
