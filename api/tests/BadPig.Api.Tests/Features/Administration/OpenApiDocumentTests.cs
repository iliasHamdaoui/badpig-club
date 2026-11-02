using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class OpenApiDocumentTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Route = $"{V1}/openapi/v1.json";

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
    public async Task Should_DescribeEveryEndpoint_When_SignedInPersonIsOnTheBoard()
    {
        var response = await As(GivenBoardMember()).GetAsync(Route, CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paths = (await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(CancellationToken)).GetProperty("paths");
        Assert.True(paths.TryGetProperty($"{V1}/me", out _));
        Assert.True(paths.TryGetProperty($"{V1}/admin/seasons", out _));
        Assert.True(paths.TryGetProperty($"{V1}/admin/users/{{id}}/membership/validate", out _));
    }
}
