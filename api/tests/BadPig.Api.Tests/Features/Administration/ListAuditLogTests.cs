using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class ListAuditLogTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/admin/audit-log";

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
    public async Task Should_RecordWhoDidWhat_When_BoardActsOnAnAccount()
    {
        var board = GivenBoardMember("Paul Bureau");
        var member = GivenMember();

        await As(board).PostAsync($"{V1}/admin/users/{member.Id}/disable", null, CancellationToken);

        var log = await ReadAsync<PageDto<AuditEntryDto>>(await As(board).GetAsync(Route, CancellationToken));
        Assert.Contains(log.Items, e => e.Actor == "Paul Bureau" && e.TargetId == member.Id);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_SortFieldIsUnknown()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{Route}?sort=unknown", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
