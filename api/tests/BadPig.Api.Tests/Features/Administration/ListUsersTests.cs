using BadPig.Api.Tests.Infrastructure;
using BadPig.Shared.Core;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class ListUsersTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/admin/users";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync(Route, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsOnlyAMember()
    {
        var response = await As(GivenMember()).GetAsync(Route, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ListClubMembersByName_When_NoSortIsGiven()
    {
        var board = GivenBoardMember("Paul Bureau");
        GivenMember("Zoe Membre");
        GivenMember("Alain Membre");

        var page = await ReadAsync<PageDto<UserDto>>(await As(board).GetAsync(Route, CancellationToken));

        Assert.Equal(["Alain Membre", "Paul Bureau", "Zoe Membre"], page.Items.Select(u => u.Name));
    }

    [Fact]
    public async Task Should_NotListTechnicalAccounts_When_TheyBelongToNoClubGroup()
    {
        var board = GivenBoardMember();
        Api.Accounts.Add(new AccountInfo(Guid.NewGuid().ToString(), "akadmin", "Technical", "tech@example.test", true, ["authentik Admins"], null));

        var page = await ReadAsync<PageDto<UserDto>>(await As(board).GetAsync(Route, CancellationToken));

        Assert.DoesNotContain(page.Items, u => u.Name == "Technical");
    }

    [Fact]
    public async Task Should_FindPeopleByNameOrEmail_When_SearchIsGiven()
    {
        var board = GivenBoardMember();
        GivenMember("Marie Dupont");
        GivenMember("Olivier Martin");

        var byName = await ReadAsync<PageDto<UserDto>>(await As(board).GetAsync($"{Route}?search=dupont", CancellationToken));
        var byEmail = await ReadAsync<PageDto<UserDto>>(await As(board).GetAsync($"{Route}?search=olivier.martin@", CancellationToken));

        Assert.Equal(["Marie Dupont"], byName.Items.Select(u => u.Name));
        Assert.Equal(["Olivier Martin"], byEmail.Items.Select(u => u.Name));
    }

    [Fact]
    public async Task Should_ReturnTheRequestedPageOnly_When_PageSizeIsGiven()
    {
        var board = GivenBoardMember("Paul Bureau");
        GivenMember("Alain Membre");
        GivenMember("Zoe Membre");

        var page = await ReadAsync<PageDto<UserDto>>(await As(board).GetAsync($"{Route}?page=2&pageSize=2", CancellationToken));

        Assert.Equal(["Zoe Membre"], page.Items.Select(u => u.Name));
        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.TotalPages);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_PageSizeIsTooLarge()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{Route}?pageSize=1000", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_SortFieldIsUnknown()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{Route}?sort=password", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_NotAllowManagingMyOwnAccount_When_IAmInTheList()
    {
        var board = GivenBoardMember();

        var me = await UserAsync(board, board);

        Assert.False(me.CanManage);
    }

    [Fact]
    public async Task Should_AllowManagingMembersButNotAdministrators_When_SignedInPersonIsOnlyOnTheBoard()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        var admin = GivenAdmin();

        Assert.True((await UserAsync(board, member)).CanManage);
        Assert.False((await UserAsync(board, admin)).CanManage);
    }

    [Fact]
    public async Task Should_AllowManagingAdministrators_When_SignedInPersonIsAnAdministrator()
    {
        var admin = GivenAdmin("Alice Admin");
        var otherAdmin = GivenAdmin("Adam Admin");

        Assert.True((await UserAsync(admin, otherAdmin)).CanManage);
    }

    [Fact]
    public async Task Should_ReportThePersonAsWaitingForSignup_When_TheyHaveNeverSignedIn()
    {
        var board = GivenBoardMember();
        var invited = GivenInvitedPerson(); // an account of the identity provider, without an invitation line

        var page = await ReadAsync<PageDto<UserDto>>(await As(board).GetAsync(Route, CancellationToken));

        Assert.Equal("pending", page.Items.Single(u => u.Id == invited.Id).Status);
        Assert.Equal("active", page.Items.Single(u => u.Id == board.Id).Status);
    }

    [Fact]
    public async Task Should_KeepOnlyThePlainMembers_When_TheGroupFilterIsMembers()
    {
        var board = GivenBoardMember("Paul Bureau");
        GivenAdmin("Alice Admin");
        GivenMember("Zoe Membre");

        var page = await ReadAsync<PageDto<UserDto>>(await As(board).GetAsync($"{Route}?group=members", CancellationToken));

        Assert.Equal(["Zoe Membre"], page.Items.Select(u => u.Name));
        Assert.Equal(1, page.TotalCount);
    }

    [Fact]
    public async Task Should_KeepTheBoardAndTheAdmins_When_TheGroupFilterIsBoard()
    {
        var board = GivenBoardMember("Paul Bureau");
        GivenAdmin("Alice Admin");
        GivenMember("Zoe Membre");

        var page = await ReadAsync<PageDto<UserDto>>(await As(board).GetAsync($"{Route}?group=board", CancellationToken));

        Assert.Equal(["Alice Admin", "Paul Bureau"], page.Items.Select(u => u.Name));
    }

    [Fact]
    public async Task Should_KeepOnlyTheAdmins_When_TheGroupFilterIsAdmin()
    {
        var board = GivenBoardMember("Paul Bureau");
        GivenAdmin("Alice Admin");

        var page = await ReadAsync<PageDto<UserDto>>(await As(board).GetAsync($"{Route}?group=admin", CancellationToken));

        Assert.Equal(["Alice Admin"], page.Items.Select(u => u.Name));
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheGroupFilterIsUnknown()
    {
        var board = GivenBoardMember();

        var response = await As(board).GetAsync($"{Route}?group=capitaines", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
