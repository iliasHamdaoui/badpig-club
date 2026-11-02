using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class RemoveFromBoardTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static string Route(Person person) => $"{V1}/admin/users/{person.Id}/board/remove";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsync(Route(GivenBoardMember()), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_RemoveThePersonFromTheBoardButKeepTheMembership_When_OtherBoardMembersRemain()
    {
        var board = GivenBoardMember("Paul Bureau");
        var leaving = GivenBoardMember("Bruno Bureau");

        var response = await As(board).PostAsync(Route(leaving), null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var user = await UserAsync(board, leaving);
        Assert.DoesNotContain("Bureau", user.Groups);
        Assert.Contains("Membres", user.Groups);
        AssertEmailSentTo(leaving);
    }

    [Fact]
    public async Task Should_TakeTheTitleAway_When_ThePersonLeavesTheBoard()
    {
        var board = GivenBoardMember("Paul Bureau");
        var leaving = GivenBoardMember("Bruno Bureau");
        var titleId = await GivenTitleAsync(board, "Secrétaire");
        await As(board).PutAsJsonAsync($"{V1}/admin/users/{leaving.Id}/title", new { titleId }, CancellationToken);

        await As(board).PostAsync(Route(leaving), null, CancellationToken);

        Assert.Null((await UserAsync(board, leaving)).TitleLabel);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_PersonIsTheLastBoardMember()
    {
        var admin = GivenAccount("Alice Admin", ClubGroup.Members, ClubGroup.Admin);
        var lastBoardMember = GivenBoardMember();

        var response = await As(admin).PostAsync(Route(lastBoardMember), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("Bureau", Api.Accounts.Get(lastBoardMember.Id)!.Groups);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_BoardMemberRemovesThemself()
    {
        var board = GivenBoardMember("Paul Bureau");
        GivenBoardMember("Bruno Bureau");

        var response = await As(board).PostAsync(Route(board), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_BoardMemberRemovesAnAdministrator()
    {
        var board = GivenBoardMember();
        var admin = GivenAdmin();

        var response = await As(board).PostAsync(Route(admin), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_RemoveAnAdministratorFromTheBoard_When_SignedInPersonIsAnAdministrator()
    {
        var admin = GivenAdmin("Alice Admin");
        var otherAdmin = GivenAdmin("Adam Admin");

        var response = await As(admin).PostAsync(Route(otherAdmin), null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
