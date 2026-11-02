using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class ListSeasonStatisticsTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/admin/seasons/statistics";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync(Route, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotOnTheBoard()
    {
        var response = await As(GivenMember()).GetAsync(Route, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_CountMembersAndRevenuePerKind_When_MembershipsAreValidated()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board, internalFee: 60, externalFee: 30);
        await GivenValidatedMembershipAsync(board, GivenMember("Marie Membre"), "internal");
        await GivenValidatedMembershipAsync(board, GivenMember("Olivier Autre"), "internal");
        await GivenValidatedMembershipAsync(board, GivenMember("Eric Externe"), "external");

        var statistics = (await ReadAsync<PageDto<SeasonStatisticsDto>>(await As(board).GetAsync(Route, CancellationToken))).Items.Single();

        Assert.Equal(2, statistics.InternalCount);
        Assert.Equal(1, statistics.ExternalCount);
        Assert.Equal(3, statistics.TotalCount);
        Assert.Equal(120, statistics.InternalRevenue);
        Assert.Equal(30, statistics.ExternalRevenue);
        Assert.Equal(150, statistics.TotalRevenue);
    }

    [Fact]
    public async Task Should_ListEverySeason_When_SeveralSeasonsExist()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board, startsOn: "2025-09-01");
        await GivenSeasonAsync(board, startsOn: "2026-09-01");

        var statistics = await ReadAsync<PageDto<SeasonStatisticsDto>>(await As(board).GetAsync(Route, CancellationToken));

        Assert.Equal(2, statistics.TotalCount);
    }
}
