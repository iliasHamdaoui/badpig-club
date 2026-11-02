using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class ValidateMembershipTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static string Route(Person person) => $"{V1}/admin/users/{person.Id}/membership/validate";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PostAsJsonAsync(Route(GivenMember()), new { }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember("Marie Membre")).PostAsJsonAsync(Route(GivenMember("Olivier Autre")), new { }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_BoardMemberValidatesOwnMembership()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board);

        var response = await As(board).PostAsJsonAsync(Route(board), new { }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_AccountDoesNotExist()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board);

        var response = await As(board).PostAsJsonAsync($"{V1}/admin/users/{Guid.NewGuid()}/membership/validate", new { }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_NoSeasonExists()
    {
        var response = await As(GivenBoardMember()).PostAsJsonAsync(Route(GivenMember()), new { }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_AccountIsDisabled()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board);

        var response = await As(board).PostAsJsonAsync(Route(GivenDisabledMember()), new { }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_KindIsUnknown()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board);

        var response = await As(board).PostAsJsonAsync(Route(GivenMember()), new { kind = "vip" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ChargeTheInternalFee_When_NoKindIsGiven()
    {
        var board = GivenBoardMember("Paul Bureau");
        var member = GivenMember();
        await GivenSeasonAsync(board, internalFee: 60, externalFee: 30);

        var response = await As(board).PostAsJsonAsync(Route(member), new { }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var user = await UserAsync(board, member);
        Assert.Equal("valid", user.Membership);
        Assert.Equal("2026-2027", user.MembershipSeason);
        Assert.Equal("internal", user.MembershipKind);
        Assert.Equal(60, user.MembershipAmount);
        Assert.Equal("Paul Bureau", user.MembershipValidatedByName);
    }

    [Fact]
    public async Task Should_ChargeTheExternalFee_When_KindIsExternal()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        await GivenSeasonAsync(board, internalFee: 60, externalFee: 30);

        await As(board).PostAsJsonAsync(Route(member), new { kind = "external" }, CancellationToken);

        var user = await UserAsync(board, member);
        Assert.Equal("external", user.MembershipKind);
        Assert.Equal(30, user.MembershipAmount);
    }

    [Fact]
    public async Task Should_ChargeTheExternalFee_When_PersonWasInvitedAsExternal()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board, internalFee: 60, externalFee: 30);
        var invited = await ReadAsync<IdDto>(await As(board).PostAsJsonAsync($"{V1}/admin/invitations",
            new { name = "Eric Externe", email = "eric.externe@example.test", membershipKind = "external" }, CancellationToken));
        var person = new Person(invited.Id, "Eric Externe", "eric.externe@example.test", ["Membres"]);

        await As(board).PostAsJsonAsync(Route(person), new { }, CancellationToken);

        Assert.Equal(30, (await UserAsync(board, person)).MembershipAmount);
    }

    [Fact]
    public async Task Should_NotChargeTwice_When_MembershipIsAlreadyValidated()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        await GivenSeasonAsync(board, internalFee: 60, externalFee: 30);
        await GivenValidatedMembershipAsync(board, member, "internal");

        var response = await As(board).PostAsJsonAsync(Route(member), new { kind = "external" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(60, (await UserAsync(board, member)).MembershipAmount);
    }

    [Fact]
    public async Task Should_AppearInTheAuditLog_When_MembershipIsValidated()
    {
        var board = GivenBoardMember("Paul Bureau");
        var member = GivenMember();
        await GivenSeasonAsync(board);

        await GivenValidatedMembershipAsync(board, member);

        var log = await ReadAsync<PageDto<AuditEntryDto>>(await As(board).GetAsync($"{V1}/admin/audit-log", CancellationToken));
        Assert.Contains(log.Items, e => e.Action == "membership.validated.2026-2027" && e.TargetId == member.Id && e.Actor == "Paul Bureau");
    }
}
