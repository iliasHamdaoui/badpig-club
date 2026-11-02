using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class DeleteSeasonTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/admin/seasons";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{Route}/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_DeleteTheSeason_When_NoMembershipWasValidated()
    {
        var board = GivenBoardMember();
        var seasonId = await GivenSeasonAsync(board);

        var response = await As(board).DeleteAsync($"{Route}/{seasonId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty((await ReadAsync<PageDto<SeasonDto>>(await As(board).GetAsync(Route, CancellationToken))).Items);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_SeasonHasValidatedMemberships()
    {
        var board = GivenBoardMember();
        var seasonId = await GivenSeasonAsync(board);
        await GivenValidatedMembershipAsync(board, GivenMember());

        var response = await As(board).DeleteAsync($"{Route}/{seasonId}", CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_SeasonHasInterclubTeams()
    {
        var board = GivenBoardMember();
        var seasonId = await GivenSeasonAsync(board);
        await GivenTeamAsync(board, "D4", seasonId);

        var response = await As(board).DeleteAsync($"{Route}/{seasonId}", CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_DeletedSeasonDoesNotExist()
    {
        var response = await As(GivenBoardMember()).DeleteAsync($"{Route}/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
