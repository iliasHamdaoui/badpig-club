using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class CreateDateProposalTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static object Body(string opponent = "BC Lyon", string venue = "home", params string[] dates) =>
        new { opponent, venue, dates = (dates.Length > 0 ? dates : ["2026-11-14", "2026-11-21"]).Select(d => new { date = d }).ToArray() };

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync($"{V1}/interclubs/teams/{Guid.NewGuid()}/proposals", Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TeamDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync($"{V1}/interclubs/teams/{Guid.NewGuid()}/proposals", Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotTheCaptainOfTheTeam()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var teamId = await GivenTeamAsync(board);

        var response = await As(adherent).PostAsJsonAsync($"{V1}/interclubs/teams/{teamId}/proposals", Body(), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_FewerThanTwoDatesAreOffered()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);

        var response = await As(board).PostAsJsonAsync($"{V1}/interclubs/teams/{teamId}/proposals", Body(dates: "2026-11-14"), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_DatesAreTheSame()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);

        var response = await As(board).PostAsJsonAsync($"{V1}/interclubs/teams/{teamId}/proposals", Body(dates: ["2026-11-14", "2026-11-14"]), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_MoreThanTenDatesAreOffered()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);
        var dates = Enumerable.Range(1, 11).Select(day => $"2026-11-{day:00}").ToArray();

        var response = await As(board).PostAsJsonAsync($"{V1}/interclubs/teams/{teamId}/proposals", Body(dates: dates), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_OpponentOrVenueIsInvalid()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);
        var url = $"{V1}/interclubs/teams/{teamId}/proposals";

        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PostAsJsonAsync(url, Body(opponent: " "), CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PostAsJsonAsync(url, Body(venue: "neutral"), CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_CreateTheOpenProposal_When_TheCaptainOffersThreeDates()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var teamId = await GivenTeamAsync(board, "D4", captain: captain);

        var response = await As(captain).PostAsJsonAsync($"{V1}/interclubs/teams/{teamId}/proposals",
            Body(" BC Lyon ", "away", "2026-11-28", "2026-11-14", "2026-11-21"), CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var proposal = await ProposalAsync(captain, (await ReadAsync<IdDto>(response)).Id);
        Assert.Equal("D4", proposal.TeamName);
        Assert.Equal("BC Lyon", proposal.Opponent);
        Assert.Equal("away", proposal.Venue);
        Assert.True(proposal.IsOpen);
        Assert.True(proposal.CanManage);
        Assert.Equal(["2026-11-14", "2026-11-21", "2026-11-28"], proposal.Dates.Select(d => d.Date));
    }

    [Fact]
    public async Task Should_AcceptTheSameDayAtTwoTimes_When_TheTimesDiffer()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);

        var response = await As(board).PostAsJsonAsync($"{V1}/interclubs/teams/{teamId}/proposals",
            new { opponent = "BC Lyon", venue = "home", dates = new[] { new { date = "2026-11-14", startTime = "10:00" }, new { date = "2026-11-14", startTime = "14:00" } } }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
