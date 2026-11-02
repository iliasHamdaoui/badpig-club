using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class EnableAccountTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsync($"{V1}/admin/users/{GivenDisabledMember().Id}/enable", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_EnableTheAccount_When_BoardReenablesADisabledMember()
    {
        var board = GivenBoardMember();
        var member = GivenDisabledMember();

        var response = await As(board).PostAsync($"{V1}/admin/users/{member.Id}/enable", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True((await UserAsync(board, member)).IsActive);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember("Marie Membre")).PostAsync($"{V1}/admin/users/{GivenDisabledMember().Id}/enable", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
