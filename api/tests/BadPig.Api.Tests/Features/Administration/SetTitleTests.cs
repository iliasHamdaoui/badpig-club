using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class SetTitleTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static string Route(Person person) => $"{V1}/admin/users/{person.Id}/title";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync(Route(GivenBoardMember()), new { titleId = (string?)null }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_GiveTheTitleAndNotifyThePerson_When_PersonIsAnActiveBoardMember()
    {
        var board = GivenBoardMember("Paul Bureau");
        var other = GivenBoardMember("Bruno Bureau");
        var titleId = await GivenTitleAsync(board, "Trésorier(e)");

        var response = await As(board).PutAsJsonAsync(Route(other), new { titleId }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("Trésorier(e)", (await UserAsync(board, other)).TitleLabel);
        AssertEmailSentTo(other);
    }

    [Fact]
    public async Task Should_MoveTheTitleToTheNewHolder_When_TitleAlreadyHadAHolder()
    {
        var board = GivenBoardMember("Paul Bureau");
        var previousHolder = GivenBoardMember("Bruno Bureau");
        var newHolder = GivenBoardMember("Berthe Bureau");
        var titleId = await GivenTitleAsync(board);
        await As(board).PutAsJsonAsync(Route(previousHolder), new { titleId }, CancellationToken);

        await As(board).PutAsJsonAsync(Route(newHolder), new { titleId }, CancellationToken);

        Assert.Null((await UserAsync(board, previousHolder)).TitleLabel);
        Assert.Equal("Président(e)", (await UserAsync(board, newHolder)).TitleLabel);
    }

    [Fact]
    public async Task Should_TakeTheTitleAway_When_NoTitleIsGiven()
    {
        var board = GivenBoardMember();
        var titleId = await GivenTitleAsync(board);
        await As(board).PutAsJsonAsync(Route(board), new { titleId }, CancellationToken);

        var response = await As(board).PutAsJsonAsync(Route(board), new { titleId = (string?)null }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null((await UserAsync(board, board)).TitleLabel);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_PersonIsNotOnTheBoard()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        var titleId = await GivenTitleAsync(board);

        var response = await As(board).PutAsJsonAsync(Route(member), new { titleId }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TitleDoesNotExist()
    {
        var board = GivenBoardMember();

        var response = await As(board).PutAsJsonAsync(Route(board), new { titleId = Guid.NewGuid().ToString() }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
