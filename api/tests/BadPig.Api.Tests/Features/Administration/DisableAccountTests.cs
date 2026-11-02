using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class DisableAccountTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static string Route(Person person) => $"{V1}/admin/users/{person.Id}/disable";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsync(Route(GivenMember()), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_DisableTheAccountAndNotifyThePerson_When_BoardDisablesAMember()
    {
        var board = GivenBoardMember();
        var member = GivenMember();

        var response = await As(board).PostAsync(Route(member), null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var user = await UserAsync(board, member);
        Assert.False(user.IsActive);
        Assert.Equal("disabled", user.Status);
        AssertEmailSentTo(member);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_BoardMemberDisablesOwnAccount()
    {
        var board = GivenBoardMember();

        var response = await As(board).PostAsync(Route(board), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.True(Api.Accounts.Get(board.Id)!.IsActive);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_BoardMemberDisablesAnAdministrator()
    {
        var response = await As(GivenBoardMember()).PostAsync(Route(GivenAdmin()), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_DisableTheAdministrator_When_SignedInPersonIsAnAdministrator()
    {
        var response = await As(GivenAdmin("Alice Admin")).PostAsync(Route(GivenAdmin("Adam Admin")), null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember("Marie Membre")).PostAsync(Route(GivenMember("Olivier Autre")), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AccountDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PostAsync($"{V1}/admin/users/{Guid.NewGuid()}/disable", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ShowTheAutomaticDeletionDate_When_AccountIsDisabled()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        await As(board).PostAsync(Route(member), null, CancellationToken);

        var page = await ReadAsync<PageDto<System.Text.Json.JsonElement>>(await As(board).GetAsync($"{V1}/admin/users?search={member.Name}", CancellationToken));

        var deletionAt = page.Items.Single().GetProperty("automaticDeletionAt");
        Assert.Equal(DateTimeOffset.Parse("2026-11-15T10:00:00Z"), deletionAt.GetDateTimeOffset());
    }
}
