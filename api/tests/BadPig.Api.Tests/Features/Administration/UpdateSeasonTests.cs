using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class UpdateSeasonTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/admin/seasons";

    // A full replacement (PUT): every field of the 2026-2027 season is sent.
    private static object Season2026(decimal internalFee, decimal externalFee) => new
    {
        name = "2026-2027", startsOn = "2026-09-01", endsOn = "2027-08-31", graceDays = 30, internalFee, externalFee,
    };

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync($"{Route}/{Guid.NewGuid()}", Season2026(70, 30), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ChangeTheFees_When_BoardUpdatesTheSeason()
    {
        var board = GivenBoardMember();
        var seasonId = await GivenSeasonAsync(board);

        var response = await As(board).PutAsJsonAsync($"{Route}/{seasonId}", Season2026(70, 35), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var season = (await ReadAsync<PageDto<SeasonDto>>(await As(board).GetAsync(Route, CancellationToken))).Items.Single();
        Assert.Equal(70, season.InternalFee);
        Assert.Equal(35, season.ExternalFee);
    }

    [Fact]
    public async Task Should_KeepTheAmountAlreadyCharged_When_TheFeeChangesAfterValidation()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        var seasonId = await GivenSeasonAsync(board, internalFee: 60);
        await GivenValidatedMembershipAsync(board, member);

        await As(board).PutAsJsonAsync($"{Route}/{seasonId}", Season2026(70, 30), CancellationToken);

        Assert.Equal(60, (await UserAsync(board, member)).MembershipAmount);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_AFieldIsMissing()
    {
        var board = GivenBoardMember();
        var seasonId = await GivenSeasonAsync(board);

        var response = await As(board).PutAsJsonAsync($"{Route}/{seasonId}", new { internalFee = 70 }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_UpdatedSeasonDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PutAsJsonAsync($"{Route}/{Guid.NewGuid()}", Season2026(70, 30), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
