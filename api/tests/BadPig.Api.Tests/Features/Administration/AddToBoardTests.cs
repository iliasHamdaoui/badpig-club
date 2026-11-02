using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class AddToBoardTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static string Route(Person person) => $"{V1}/admin/users/{person.Id}/board/add";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsync(Route(GivenMember()), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember("Marie Membre")).PostAsync(Route(GivenMember("Olivier Autre")), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_AddThePersonToTheBoardAndNotifyThem_When_TheyHaveSignedIn()
    {
        var board = GivenBoardMember();
        var member = GivenMember();

        var response = await As(board).PostAsync(Route(member), null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Contains("Bureau", (await UserAsync(board, member)).Groups);
        AssertEmailSentTo(member);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonHasNeverSignedIn()
    {
        var board = GivenBoardMember();

        var response = await As(board).PostAsync(Route(GivenInvitedPerson()), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        AssertNoEmailSent();
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_AccountIsDisabled()
    {
        var response = await As(GivenBoardMember()).PostAsync(Route(GivenDisabledMember()), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AccountDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PostAsync($"{V1}/admin/users/{Guid.NewGuid()}/board/add", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AccountIsNotAClubMember()
    {
        var technical = GivenAccount("Compte technique");
        Api.Accounts.Add(Api.Accounts.Get(technical.Id)! with { Groups = ["authentik Admins"] });

        var response = await As(GivenBoardMember()).PostAsync(Route(technical), null, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
