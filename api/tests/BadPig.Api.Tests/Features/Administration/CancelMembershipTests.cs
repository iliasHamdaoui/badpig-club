using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class CancelMembershipTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static string Route(Person person) => $"{V1}/admin/users/{person.Id}/membership/cancel";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsync(Route(GivenMember()), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_PutMembershipBackToValidate_When_BoardCancelsIt()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        await GivenSeasonAsync(board);
        await GivenValidatedMembershipAsync(board, member);

        var response = await As(board).PostAsync(Route(member), null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("to_validate", (await UserAsync(board, member)).Membership);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_BoardMemberCancelsOwnMembership()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board);

        var response = await As(board).PostAsync(Route(board), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember("Marie Membre")).PostAsync(Route(GivenMember("Olivier Autre")), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNoContent_When_MembershipWasNeverValidated()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board);

        var response = await As(board).PostAsync(Route(GivenMember()), null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
