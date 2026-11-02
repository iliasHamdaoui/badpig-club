using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class AddTeamMemberTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string Url(string teamId, string accountId) => $"{V1}/interclubs/teams/{teamId}/members/{accountId}";

    private async Task<List<TeamMemberDto>> MembersAsync(Person reader, string teamId) =>
        (await ReadAsync<PageDto<TeamMemberDto>>(await As(reader).GetAsync($"{V1}/interclubs/teams/{teamId}/members", CancellationToken))).Items;

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsync(Url(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TeamDoesNotExist()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);

        var response = await As(board).PutAsync(Url(Guid.NewGuid().ToString(), adherent.Id), null, CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotTheCaptainOfTheTeam()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var teamId = await GivenTeamAsync(board);

        var response = await As(adherent).PutAsync(Url(teamId, adherent.Id), null, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Empty(await MembersAsync(board, teamId));
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_IdentifierIsInvalidOrPersonUnknownOrDisabled()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);

        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsync(Url(teamId, "not-an-identifier"), null, CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsync(Url(teamId, Guid.NewGuid().ToString()), null, CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await As(board).PutAsync(Url(teamId, GivenDisabledMember().Id), null, CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task Should_AddThePlayer_When_TheCaptainAddsAnAdherent()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var anna = await GivenAnAdherentAsync(board, "Anna Adherente");
        var teamId = await GivenTeamAsync(board, captain: captain);

        var response = await As(captain).PutAsync(Url(teamId, anna.Id), null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("Anna Adherente", Assert.Single(await MembersAsync(captain, teamId)).Name);
    }

    [Fact]
    public async Task Should_AddThePlayerOnce_When_TheBoardAddsThemTwice()
    {
        var board = GivenBoardMember();
        var anna = await GivenAnAdherentAsync(board, "Anna Adherente");
        var teamId = await GivenTeamAsync(board);

        await GivenPlayerAsync(board, teamId, anna);
        var again = await As(board).PutAsync(Url(teamId, anna.Id), null, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, again.StatusCode);
        Assert.Single(await MembersAsync(board, teamId));
    }

    [Fact]
    public async Task Should_LetTheCaptainBeAPlayerToo_When_TheyAddThemselves()
    {
        var board = GivenBoardMember();
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var teamId = await GivenTeamAsync(board, captain: captain);

        await GivenPlayerAsync(captain, teamId, captain);

        var member = Assert.Single(await MembersAsync(captain, teamId));
        Assert.True(member.IsCaptain);
    }
}
