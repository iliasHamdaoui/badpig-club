using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class RenameTitleTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/admin/titles";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync($"{Route}/{Guid.NewGuid()}", new { label = "Secrétaire" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_RenameTheTitle_When_NewLabelIsFree()
    {
        var board = GivenBoardMember();
        var titleId = await GivenTitleAsync(board, "Secrétaire");

        var response = await As(board).PutAsJsonAsync($"{Route}/{titleId}", new { label = "Secrétaire général" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var titles = await ReadAsync<PageDto<TitleDto>>(await As(board).GetAsync(Route, CancellationToken));
        Assert.Equal("Secrétaire général", titles.Items.Single().Label);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_NewLabelBelongsToAnotherTitle()
    {
        var board = GivenBoardMember();
        await GivenTitleAsync(board, "Secrétaire");
        var treasurer = await GivenTitleAsync(board, "Trésorier(e)");

        var response = await As(board).PutAsJsonAsync($"{Route}/{treasurer}", new { label = "Secrétaire" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
