using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class UpdateFixtureTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static object Body(string? date = "2026-11-21", string? opponent = "BC Nantes", string? venue = "away") =>
        new { date, startTime = "10:00", opponent, venue, place = "Gymnase de Nantes" };

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync($"{V1}/interclubs/fixtures/{Guid.NewGuid()}", Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_FixtureDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PutAsJsonAsync($"{V1}/interclubs/fixtures/{Guid.NewGuid()}", Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotTheCaptainOfTheTeam()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var id = await GivenFixtureAsync(board, await GivenTeamAsync(board));

        var response = await As(adherent).PutAsJsonAsync($"{V1}/interclubs/fixtures/{id}", Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_DateOpponentOrVenueIsMissingOrInvalid()
    {
        var board = GivenBoardMember();
        var id = await GivenFixtureAsync(board, await GivenTeamAsync(board));
        var url = $"{V1}/interclubs/fixtures/{id}";

        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(url, Body(date: null), CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(url, Body(opponent: ""), CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsJsonAsync(url, Body(venue: "elsewhere"), CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_ChangeTheFixture_When_TheCaptainChangesIt()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var teamId = await GivenTeamAsync(board, captain: captain);
        var id = await GivenFixtureAsync(captain, teamId, place: "Gymnase Pignan");

        var response = await As(captain).PutAsJsonAsync($"{V1}/interclubs/fixtures/{id}", Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var fixture = await FixtureAsync(captain, id);
        Assert.Equal("2026-11-21", fixture.Date);
        Assert.Equal("10:00:00", fixture.StartTime);
        Assert.Equal("BC Nantes", fixture.Opponent);
        Assert.Equal("away", fixture.Venue);
        Assert.Equal("Gymnase de Nantes", fixture.Place);
    }

    [Fact]
    public async Task Should_ClearTheTimeAndThePlace_When_TheyAreLeftOut()
    {
        var board = GivenBoardMember();
        var id = await GivenFixtureAsync(board, await GivenTeamAsync(board), startTime: "14:00", place: "Gymnase Pignan");

        await As(board).PutAsJsonAsync($"{V1}/interclubs/fixtures/{id}", new { date = "2026-11-14", opponent = "BC Lyon", venue = "home" }, CancellationToken);

        var fixture = await FixtureAsync(board, id);
        Assert.Null(fixture.StartTime);
        Assert.Null(fixture.Place);
    }
}
