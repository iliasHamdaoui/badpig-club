using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class AssignCaptainTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync($"{V1}/admin/interclubs/teams/{Guid.NewGuid()}/captain", new { captainId = Guid.NewGuid() }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).PutAsJsonAsync($"{V1}/admin/interclubs/teams/{Guid.NewGuid()}/captain", new { captainId = Guid.NewGuid() }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TeamDoesNotExist()
    {
        var board = GivenBoardMember();
        var member = GivenMember();

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/interclubs/teams/{Guid.NewGuid()}/captain", new { captainId = member.Id }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_CaptainIdentifierIsInvalid()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/interclubs/teams/{teamId}/captain", new { captainId = "not-an-identifier" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_PersonDoesNotExist()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/interclubs/teams/{teamId}/captain", new { captainId = Guid.NewGuid() }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_PersonIsDisabled()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);
        var disabled = GivenDisabledMember();

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/interclubs/teams/{teamId}/captain", new { captainId = disabled.Id }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_DesignateTheCaptain_When_BoardMemberAssignsAnAdherent()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var teamId = await GivenTeamAsync(board);

        var response = await As(board).PutAsJsonAsync($"{V1}/admin/interclubs/teams/{teamId}/captain", new { captainId = captain.Id }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var team = Assert.Single(await TeamsAsync(board));
        Assert.Equal(captain.Id, team.CaptainId);
        Assert.Equal("Carla Capitaine", team.CaptainName);
    }

    [Fact]
    public async Task Should_ReplaceThePreviousCaptain_When_AnotherPersonIsAssigned()
    {
        var board = GivenBoardMember();
        var first = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var second = await GivenAnAdherentAsync(board, "Denis Deuxieme");
        var teamId = await GivenTeamAsync(board, captain: first);

        await As(board).PutAsJsonAsync($"{V1}/admin/interclubs/teams/{teamId}/captain", new { captainId = second.Id }, CancellationToken);

        Assert.Equal(second.Id, Assert.Single(await TeamsAsync(board)).CaptainId);
    }
}
