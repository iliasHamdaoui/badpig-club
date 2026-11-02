using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class CreateFixtureTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static object Body(string? date = "2026-11-14", string? opponent = "BC Lyon", string? venue = "home") => new { date, opponent, venue };

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync($"{V1}/interclubs/teams/{Guid.NewGuid()}/fixtures", Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TeamDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync($"{V1}/interclubs/teams/{Guid.NewGuid()}/fixtures", Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsAnOrdinaryAdherent()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var teamId = await GivenTeamAsync(board);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/interclubs/teams/{teamId}/fixtures", Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsTheCaptainOfAnotherTeam()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        await GivenTeamAsync(board, "D4", captain: captain);
        var otherTeam = await GivenTeamAsync(board, "D5");

        var response = await As(captain).PostAsJsonAsync($"{V1}/interclubs/teams/{otherTeam}/fixtures", Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_DateOpponentOrVenueIsMissingOrInvalid()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);
        var url = $"{V1}/interclubs/teams/{teamId}/fixtures";

        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PostAsJsonAsync(url, Body(date: null), CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PostAsJsonAsync(url, Body(opponent: " "), CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PostAsJsonAsync(url, Body(venue: "neutral"), CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PostAsJsonAsync(url, Body(venue: null), CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_CreateTheFixture_When_TheCaptainCreatesIt()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var teamId = await GivenTeamAsync(board, "D4", captain: captain);

        var response = await As(captain).PostAsJsonAsync($"{V1}/interclubs/teams/{teamId}/fixtures",
            new { date = "2026-11-14", startTime = "14:00", opponent = " BC Lyon ", venue = "away", place = " Gymnase de Lyon " }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = Assert.Single(await FixturesAsync(captain));
        Assert.Equal((await ReadAsync<IdDto>(response)).Id, created.Id);
        Assert.Equal(teamId, created.TeamId);
        Assert.Equal("2026-11-14", created.Date);
        Assert.Equal("14:00:00", created.StartTime);
        Assert.Equal("BC Lyon", created.Opponent);
        Assert.Equal("away", created.Venue);
        Assert.Equal("Gymnase de Lyon", created.Place);
    }

    [Fact]
    public async Task Should_CreateTheFixture_When_ABoardMemberCreatesItForATeamWithoutCaptain()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);

        var response = await As(board).PostAsJsonAsync($"{V1}/interclubs/teams/{teamId}/fixtures", Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
