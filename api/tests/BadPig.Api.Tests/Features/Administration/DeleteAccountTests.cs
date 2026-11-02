using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class DeleteAccountTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync($"{V1}/admin/users/{GivenMember().Id}", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_DeleteTheAccountAndEraseItsMembership_When_BoardDeletesAMember()
    {
        var board = GivenBoardMember();
        var member = GivenMember();
        await GivenSeasonAsync(board);
        await GivenValidatedMembershipAsync(board, member);

        var response = await As(board).DeleteAsync($"{V1}/admin/users/{member.Id}", CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(Api.Accounts.Get(member.Id));
        var statistics = (await ReadAsync<PageDto<SeasonStatisticsDto>>(await As(board).GetAsync($"{V1}/admin/seasons/statistics", CancellationToken))).Items.Single();
        Assert.Equal(0, statistics.TotalCount);
    }

    [Fact]
    public async Task Should_KeepNoPersonalDataInTheAuditLog_When_AnAccountIsDeleted()
    {
        var board = GivenBoardMember();
        var member = GivenMember("Marie Membre");
        await As(board).DeleteAsync($"{V1}/admin/users/{member.Id}", CancellationToken);

        var log = await As(board).GetStringAsync($"{V1}/admin/audit-log", CancellationToken);

        Assert.DoesNotContain("Marie Membre", log);
        Assert.DoesNotContain(member.Email, log);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_BoardMemberDeletesOwnAccountThroughTheBoardRoute()
    {
        var board = GivenBoardMember();

        var response = await As(board).DeleteAsync($"{V1}/admin/users/{board.Id}", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.NotNull(Api.Accounts.Get(board.Id));
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_BoardMemberDeletesAnAdministrator()
    {
        var response = await As(GivenBoardMember()).DeleteAsync($"{V1}/admin/users/{GivenAdmin().Id}", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
