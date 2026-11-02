using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class GetFixtureTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync($"{V1}/interclubs/fixtures/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_MembershipIsNotValidated()
    {
        var response = await As(GivenMember()).GetAsync($"{V1}/interclubs/fixtures/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_FixtureDoesNotExist()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{V1}/interclubs/fixtures/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_IdentifierIsInvalid()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{V1}/interclubs/fixtures/not-an-identifier", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnTheFixtureWithoutManagementRight_When_AnOrdinaryAdherentReadsIt()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var teamId = await GivenTeamAsync(board, "D4", captain: captain);
        var id = await GivenFixtureAsync(captain, teamId, "2026-11-14", "BC Lyon", "away", "09:30", "Gymnase de Lyon");

        var fixture = await FixtureAsync(adherent, id);

        Assert.Equal(teamId, fixture.TeamId);
        Assert.Equal("D4", fixture.TeamName);
        Assert.Equal("2026-2027", fixture.Season);
        Assert.Equal("Carla Capitaine", fixture.CaptainName);
        Assert.Equal("2026-11-14", fixture.Date);
        Assert.Equal("09:30:00", fixture.StartTime);
        Assert.Equal("BC Lyon", fixture.Opponent);
        Assert.Equal("away", fixture.Venue);
        Assert.Equal("Gymnase de Lyon", fixture.Place);
        Assert.False(fixture.CanManage);
    }

    [Fact]
    public async Task Should_AllowManagement_When_TheCaptainOrTheBoardReadsIt()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var teamId = await GivenTeamAsync(board, captain: captain);
        var id = await GivenFixtureAsync(captain, teamId);

        Assert.True((await FixtureAsync(captain, id)).CanManage);
        Assert.True((await FixtureAsync(board, id)).CanManage);
    }

    [Fact]
    public async Task Should_NotAllowManagement_When_TheCaptainReadsAFixtureOfAnotherTeam()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        await GivenTeamAsync(board, "D4", captain: captain);
        var otherTeam = await GivenTeamAsync(board, "D5");
        var id = await GivenFixtureAsync(board, otherTeam);

        Assert.False((await FixtureAsync(captain, id)).CanManage);
    }
}
