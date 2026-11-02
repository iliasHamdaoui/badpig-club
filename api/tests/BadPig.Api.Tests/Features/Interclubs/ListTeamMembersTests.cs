using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class ListTeamMembersTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string Url(string teamId, string query = "") => $"{V1}/interclubs/teams/{teamId}/members{query}";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync(Url(Guid.NewGuid().ToString()), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_MembershipIsNotValidated()
    {
        var response = await As(GivenMember()).GetAsync(Url(Guid.NewGuid().ToString()), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TeamDoesNotExist()
    {
        var response = await As(GivenBoardMember()).GetAsync(Url(Guid.NewGuid().ToString()), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_IdentifierIsInvalidOrSortIsUnknown()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);

        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).GetAsync(Url("not-an-identifier"), CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).GetAsync(Url(teamId, "?sort=score"), CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_ListThePlayersByName_When_AnAdherentAsks()
    {
        var board = GivenBoardMember();
        var reader = await GivenAnAdherentAsync(board);
        var bob = await GivenAnAdherentAsync(board, "Bob Adherent");
        var anna = await GivenAnAdherentAsync(board, "Anna Adherente");
        var teamId = await GivenTeamAsync(board);
        await GivenPlayerAsync(board, teamId, bob);
        await GivenPlayerAsync(board, teamId, anna);

        var page = await ReadAsync<PageDto<TeamMemberDto>>(await As(reader).GetAsync(Url(teamId), CancellationToken));

        Assert.Equal(["Anna Adherente", "Bob Adherent"], page.Items.Select(m => m.Name));
        Assert.Equal(2, page.TotalCount);
    }

    [Fact]
    public async Task Should_SearchOnTheName()
    {
        var board = GivenBoardMember();
        var bob = await GivenAnAdherentAsync(board, "Bob Adherent");
        var anna = await GivenAnAdherentAsync(board, "Anna Adherente");
        var teamId = await GivenTeamAsync(board);
        await GivenPlayerAsync(board, teamId, bob);
        await GivenPlayerAsync(board, teamId, anna);

        var page = await ReadAsync<PageDto<TeamMemberDto>>(await As(board).GetAsync(Url(teamId, "?search=anna"), CancellationToken));

        Assert.Equal("Anna Adherente", Assert.Single(page.Items).Name);
    }
}
