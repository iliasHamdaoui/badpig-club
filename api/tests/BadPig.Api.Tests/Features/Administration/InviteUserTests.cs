using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class InviteUserTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/admin/invitations";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync(Route, new { name = "Nina Nouvelle", email = "nina@example.test" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).PostAsJsonAsync(Route, new { name = "Nina Nouvelle", email = "nina@example.test" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_CreateTheAccountAndSendTheSignupLink_When_DataIsValid()
    {
        var board = GivenBoardMember();

        var response = await As(board).PostAsJsonAsync(Route, new { name = "Nina Nouvelle", email = "nina@example.test" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await ReadAsync<IdDto>(response);
        var account = Api.Accounts.Get(created.Id);
        Assert.NotNull(account);
        Assert.Equal("nina@example.test", account.Email);
        Assert.Equal(["Membres"], account.Groups);
        AssertSignupLinkSentTo(created.Id, times: 1);
    }

    [Fact]
    public async Task Should_ShowThePersonAsPending_When_TheyHaveNotSignedInYet()
    {
        var board = GivenBoardMember();
        var created = await ReadAsync<IdDto>(await As(board).PostAsJsonAsync(Route, new { name = "Nina Nouvelle", email = "nina@example.test" }, CancellationToken));

        var page = await ReadAsync<PageDto<UserDto>>(await As(board).GetAsync($"{V1}/admin/users?search=nina", CancellationToken));

        Assert.Equal("pending", page.Items.Single(u => u.Id == created.Id).Status);
    }

    [Fact]
    public async Task Should_AddThePersonToTheBoard_When_GroupIsBoard()
    {
        var board = GivenBoardMember();

        var created = await ReadAsync<IdDto>(await As(board).PostAsJsonAsync(Route,
            new { name = "Bruno Bureau", email = "bruno@example.test", group = "board" }, CancellationToken));

        Assert.Contains("Bureau", Api.Accounts.Get(created.Id)!.Groups);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_BoardMemberInvitesAnAdministrator()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route,
            new { name = "Adam Admin", email = "adam@example.test", group = "admin" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_InviteAnAdministrator_When_SignedInPersonIsAnAdministrator()
    {
        var response = await As(GivenAdmin()).PostAsJsonAsync(Route,
            new { name = "Adam Admin", email = "adam@example.test", group = "admin" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_EmailIsAlreadyUsedByAnAccount()
    {
        var board = GivenBoardMember();
        var existing = GivenMember("Marie Membre");

        var response = await As(board).PostAsJsonAsync(Route, new { name = "Autre Marie", email = existing.Email.ToUpperInvariant() }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequestOnTheEmailField_When_EmailIsInvalid()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, new { name = "Nina Nouvelle", email = "nina" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(CancellationToken)).GetProperty("errors");
        Assert.True(errors.TryGetProperty("email", out _));
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_GroupIsUnknown()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route,
            new { name = "Nina Nouvelle", email = "nina@example.test", group = "root" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_MembershipKindIsUnknown()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route,
            new { name = "Nina Nouvelle", email = "nina@example.test", membershipKind = "vip" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnServerErrorAndCreateNothing_When_InvitationsAreNotConfigured()
    {
        Api.Identity.CanSendSignupLink.Returns(false);

        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route, new { name = "Nina Nouvelle", email = "nina@example.test" }, CancellationToken);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Null(Api.Accounts.FindByEmail("nina@example.test"));
        _ = Api.Identity.DidNotReceive().CreateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IReadOnlyCollection<ClubGroup>>(), Arg.Any<CancellationToken>());
    }
}
