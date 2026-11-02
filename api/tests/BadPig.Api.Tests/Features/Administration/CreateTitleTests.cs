using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class CreateTitleTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/admin/titles";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync(Route, new { label = "Président(e)" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).PostAsJsonAsync(Route, new { label = "Président(e)" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnCreatedAndListTheTitle_When_LabelIsValid()
    {
        var board = GivenBoardMember();

        var response = await As(board).PostAsJsonAsync(Route, new { label = "Président(e)" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await ReadAsync<IdDto>(response);
        var titles = await ReadAsync<PageDto<TitleDto>>(await As(board).GetAsync(Route, CancellationToken));
        var title = Assert.Single(titles.Items);
        Assert.Equal(created.Id, title.Id);
        Assert.Equal("Président(e)", title.Label);
        Assert.Null(title.HolderId);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_LabelExistsWithAnotherCase()
    {
        var board = GivenBoardMember();
        await GivenTitleAsync(board, "Secrétaire");

        var response = await As(board).PostAsJsonAsync(Route, new { label = "SECRÉTAIRE" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_LabelIsEmpty()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, new { label = "  " }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
