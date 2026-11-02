using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class DeleteTitleTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/admin/titles";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{Route}/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_DeleteTheTitleAndFreeItsHolder_When_TitleIsDeleted()
    {
        var board = GivenBoardMember();
        var titleId = await GivenTitleAsync(board);
        await As(board).PutAsJsonAsync($"{V1}/admin/users/{board.Id}/title", new { titleId }, CancellationToken);

        var response = await As(board).DeleteAsync($"{Route}/{titleId}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null((await UserAsync(board, board)).TitleLabel);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_DeletedTitleDoesNotExist()
    {
        var response = await As(GivenBoardMember()).DeleteAsync($"{Route}/{Guid.NewGuid()}", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
