using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class SetMemberLicenceTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private static string Url(string id) => $"{V1}/admin/users/{id}/licence";

    private async Task<UserDto> UserAsync(Person board, string id) =>
        (await ReadAsync<PageDto<UserDto>>(await As(board).GetAsync($"{V1}/admin/users", CancellationToken))).Items.Single(u => u.Id == id);

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync(Url(Guid.NewGuid().ToString()), new { number = "07123456" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var member = GivenMember();

        var response = await As(member).PutAsJsonAsync(Url(member.Id), new { number = "07123456" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TheAccountDoesNotExist()
    {
        var response = await As(GivenBoardMember()).PutAsJsonAsync(Url(Guid.NewGuid().ToString()), new { number = "07123456" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheNumberIsNotALicenceNumber()
    {
        var board = GivenBoardMember();
        var member = GivenMember();

        var response = await As(board).PutAsJsonAsync(Url(member.Id), new { number = "07 123/456" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_RecordTheLicence_When_BoardMemberEntersIt()
    {
        var board = GivenBoardMember();
        var member = GivenMember();

        var response = await As(board).PutAsJsonAsync(Url(member.Id), new { number = " 07123456 " }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("07123456", (await UserAsync(board, member.Id)).Licence);
    }

    [Fact]
    public async Task Should_RemoveTheLicence_When_BoardMemberClearsIt()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        await As(board).PutAsJsonAsync(Url(member.Id), new { number = "07123456" }, CancellationToken);

        await As(board).PutAsJsonAsync(Url(member.Id), new { number = (string?)null }, CancellationToken);

        Assert.Null((await UserAsync(board, member.Id)).Licence);
    }

    [Fact]
    public async Task Should_FindThePerson_When_SearchingOnTheLicence()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        await As(board).PutAsJsonAsync(Url(member.Id), new { number = "07123456" }, CancellationToken);

        var found = (await ReadAsync<PageDto<UserDto>>(await As(board).GetAsync($"{V1}/admin/users?search=0712", CancellationToken))).Items;

        Assert.Equal(member.Id, Assert.Single(found).Id);
    }

    [Fact]
    public async Task Should_RecordTheChangeInTheJournal()
    {
        var board = GivenBoardMember();
        var member = GivenMember();

        await As(board).PutAsJsonAsync(Url(member.Id), new { number = "07123456" }, CancellationToken);

        var journal = (await ReadAsync<PageDto<AuditEntryDto>>(await As(board).GetAsync($"{V1}/admin/audit-log", CancellationToken))).Items;
        Assert.Contains(journal, e => e.Action == "licence.updated" && e.TargetId == member.Id);
    }

    [Fact]
    public async Task Should_ForgetTheLicence_When_TheAccountIsDeleted()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        await As(board).PutAsJsonAsync(Url(member.Id), new { number = "07123456" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, (await As(board).DeleteAsync($"{V1}/admin/users/{member.Id}", CancellationToken)).StatusCode);

        // The same person comes back later with the same account: nothing of their licence is left.
        Api.Accounts.Add(new AccountInfo(member.Id, member.Email, member.Name, member.Email, true, member.Groups, Api.Clock.UtcNow));
        Assert.Null((await UserAsync(board, member.Id)).Licence);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_AnotherMemberAlreadyHasThatLicence()
    {
        var board = GivenBoardMember();
        var marie = GivenMember("Marie Membre");
        var zoe = GivenMember("Zoe Membre");
        Assert.Equal(HttpStatusCode.NoContent, (await As(board).PutAsJsonAsync(Url(marie.Id), new { number = "07123456" }, CancellationToken)).StatusCode);

        var response = await As(board).PutAsJsonAsync(Url(zoe.Id), new { number = "07123456" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Null((await UserAsync(board, zoe.Id)).Licence);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_TheLicenceOnlyDiffersByCase()
    {
        var board = GivenBoardMember();
        var marie = GivenMember("Marie Membre");
        var zoe = GivenMember("Zoe Membre");
        await As(board).PutAsJsonAsync(Url(marie.Id), new { number = "ab-123" }, CancellationToken);

        var response = await As(board).PutAsJsonAsync(Url(zoe.Id), new { number = "AB-123" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Should_KeepTheLicence_When_TheSameNumberIsSavedAgainOnTheSamePerson()
    {
        var board = GivenBoardMember();
        var marie = GivenMember("Marie Membre");
        await As(board).PutAsJsonAsync(Url(marie.Id), new { number = "07123456" }, CancellationToken);

        var response = await As(board).PutAsJsonAsync(Url(marie.Id), new { number = "07123456" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("07123456", (await UserAsync(board, marie.Id)).Licence);
    }

    [Fact]
    public async Task Should_AllowSeveralPeopleWithoutALicence_When_TheNumberIsEmpty()
    {
        var board = GivenBoardMember();
        var marie = GivenMember("Marie Membre");
        var zoe = GivenMember("Zoe Membre");

        Assert.Equal(HttpStatusCode.NoContent, (await As(board).PutAsJsonAsync(Url(marie.Id), new { number = "" }, CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await As(board).PutAsJsonAsync(Url(zoe.Id), new { number = "" }, CancellationToken)).StatusCode);
    }
}
