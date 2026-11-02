using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Profile;

public sealed class DeleteMyAccountTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{V1}/me", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_DeleteMyAccount_When_IAmAMember()
    {
        var me = GivenMember();

        var response = await As(me).DeleteAsync($"{V1}/me", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(Api.Accounts.Get(me.Id));
    }

    [Fact]
    public async Task Should_EraseMyMembership_When_IDeleteMyAccount()
    {
        var board = GivenBoardMember();
        var me = GivenMember();
        await GivenSeasonAsync(board);
        await GivenValidatedMembershipAsync(board, me);

        await As(me).DeleteAsync($"{V1}/me", CancellationToken);

        var statistics = await ReadAsync<PageDto<SeasonStatisticsDto>>(await As(board).GetAsync($"{V1}/admin/seasons/statistics", CancellationToken));
        Assert.Equal(0, statistics.Items.Single().TotalCount);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_IAmTheLastAdministrator()
    {
        var admin = GivenAdmin();

        var response = await As(admin).DeleteAsync($"{V1}/me", CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(Api.Accounts.Get(admin.Id));
    }

    [Fact]
    public async Task Should_DeleteMyAccount_When_AnotherAdministratorRemains()
    {
        var admin = GivenAdmin("Alice Admin");
        GivenAdmin("Adam Admin");

        var response = await As(admin).DeleteAsync($"{V1}/me", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
