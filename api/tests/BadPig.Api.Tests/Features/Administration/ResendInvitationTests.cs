using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class ResendInvitationTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsync($"{V1}/admin/users/{GivenMember().Id}/resend-invitation", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_SendTheSignupLinkAgain_When_PersonHasNotSignedInYet()
    {
        var board = GivenBoardMember();
        var created = await ReadAsync<IdDto>(await As(board).PostAsJsonAsync($"{V1}/admin/invitations",
            new { name = "Nina Nouvelle", email = "nina@example.test" }, CancellationToken));

        var response = await As(board).PostAsync($"{V1}/admin/users/{created.Id}/resend-invitation", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        AssertSignupLinkSentTo(created.Id, times: 2);
    }

    [Fact]
    public async Task Should_SendTheSignupLink_When_ThePersonNeverSignedInAndWasNotInvitedFromTheSite()
    {
        var board = GivenBoardMember();
        var created = GivenInvitedPerson(); // an account of the identity provider, without an invitation line

        var response = await As(board).PostAsync($"{V1}/admin/users/{created.Id}/resend-invitation", null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        AssertSignupLinkSentTo(created.Id, times: 1);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_PersonHasAlreadySignedIn()
    {
        var board = GivenBoardMember();
        var member = GivenMember();

        var response = await As(board).PostAsync($"{V1}/admin/users/{member.Id}/resend-invitation", null, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        _ = Api.Identity.DidNotReceive().SendSignupLinkAsync(member.Id, Arg.Any<CancellationToken>());
    }
}
