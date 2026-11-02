using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class CreateSeasonTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/admin/seasons";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync(Route, new { startsOn = "2026-09-01", internalFee = 60, externalFee = 30 }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var member = GivenMember();

        var response = await As(member).PostAsJsonAsync(Route, new { startsOn = "2026-09-01", internalFee = 60, externalFee = 30 }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnCreatedWithTheIdOnly_When_DataIsValid()
    {
        var board = GivenBoardMember();

        var response = await As(board).PostAsJsonAsync(Route, new { startsOn = "2026-09-01", internalFee = 60, externalFee = 30 }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(CancellationToken);
        Assert.Equal(["id"], body.EnumerateObject().Select(p => p.Name));
    }

    [Fact]
    public async Task Should_UseTheDefaultEndNameAndGracePeriod_When_OnlyStartAndFeesAreGiven()
    {
        var board = GivenBoardMember();

        await GivenSeasonAsync(board, startsOn: "2026-09-01");

        var seasons = await ReadAsync<PageDto<SeasonDto>>(await As(board).GetAsync(Route, CancellationToken));
        var season = Assert.Single(seasons.Items);
        Assert.Equal("2026-2027", season.Name);
        Assert.Equal("2027-08-31", season.EndsOn);
        Assert.Equal(30, season.GraceDays);
    }

    [Fact]
    public async Task Should_ReturnBadRequestOnBothFees_When_FeesAreMissing()
    {
        var board = GivenBoardMember();

        var response = await As(board).PostAsJsonAsync(Route, new { startsOn = "2026-09-01" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(CancellationToken)).GetProperty("errors");
        Assert.True(errors.TryGetProperty("internalFee", out _));
        Assert.True(errors.TryGetProperty("externalFee", out _));
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_EndDateIsBeforeStartDate()
    {
        var board = GivenBoardMember();

        var response = await As(board).PostAsJsonAsync(Route,
            new { startsOn = "2026-09-01", endsOn = "2026-08-01", internalFee = 60, externalFee = 30 }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_SeasonOverlapsAnExistingOne()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board, startsOn: "2026-09-01");

        var response = await As(board).PostAsJsonAsync(Route, new { startsOn = "2027-01-01", internalFee = 60, externalFee = 30 }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_SeasonNameIsAlreadyUsed()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board, startsOn: "2026-09-01");

        var response = await As(board).PostAsJsonAsync(Route,
            new { startsOn = "2028-09-01", name = "2026-2027", internalFee = 60, externalFee = 30 }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
