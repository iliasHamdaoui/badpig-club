using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Interclubs;

public sealed class ListTeamCandidatesTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string Url(string teamId, string query = "") => $"{V1}/interclubs/teams/{teamId}/candidates{query}";

    private async Task<List<TeamCandidateDto>> CandidatesAsync(Person who, string teamId, string query = "") =>
        (await ReadAsync<PageDto<TeamCandidateDto>>(await As(who).GetAsync(Url(teamId, query), CancellationToken))).Items;

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync(Url(Guid.NewGuid().ToString()), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_TeamDoesNotExist()
    {
        var response = await As(GivenBoardMember()).GetAsync(Url(Guid.NewGuid().ToString()), CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_PersonIsNotTheCaptainOfTheTeam()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);
        var teamId = await GivenTeamAsync(board);

        var response = await As(adherent).GetAsync(Url(teamId), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_SortIsUnknown()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);

        var response = await As(board).GetAsync(Url(teamId, "?sort=score"), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ListEveryActiveMemberWhoIsNotInTheTeam_When_TheCaptainAsks()
    {
        var board = GivenBoardMember("Paul Bureau");
        var captain = await GivenAnAdherentAsync(board, "Carla Capitaine");
        var anna = await GivenAnAdherentAsync(board, "Anna Adherente");
        var bob = await GivenAnAdherentAsync(board, "Bob Adherent");
        GivenDisabledMember("Denis Desactive");
        var teamId = await GivenTeamAsync(board, captain: captain);
        await GivenPlayerAsync(captain, teamId, bob);

        var candidates = await CandidatesAsync(captain, teamId);

        Assert.Equal(["Anna Adherente", "Carla Capitaine", "Paul Bureau"], candidates.Select(c => c.Name));
        Assert.DoesNotContain(candidates, c => c.AccountId == bob.Id);
        Assert.Contains(candidates, c => c.AccountId == anna.Id);
    }

    [Fact]
    public async Task Should_SearchOnTheName()
    {
        var board = GivenBoardMember();
        await GivenAnAdherentAsync(board, "Anna Adherente");
        await GivenAnAdherentAsync(board, "Bob Adherent");
        var teamId = await GivenTeamAsync(board);

        var candidates = await CandidatesAsync(board, teamId, "?search=bob");

        Assert.Equal("Bob Adherent", Assert.Single(candidates).Name);
    }

    [Fact]
    public async Task Should_SortAlphabeticallyIgnoringCaseAndAccents_When_NoSortIsGiven()
    {
        var board = GivenBoardMember();
        var teamId = await GivenTeamAsync(board);
        await GivenAnAdherentAsync(board, "Zoé Zèbre");
        await GivenAnAdherentAsync(board, "Émilie Étoile");
        await GivenAnAdherentAsync(board, "dora Dupont");
        await GivenAnAdherentAsync(board, "Fanny Faure");

        // The board member who sets the situation up is a candidate too (he can play): he is left out of the comparison.
        var names = (await CandidatesAsync(board, teamId)).Select(c => c.Name).Where(n => n != board.Name);

        Assert.Equal(["dora Dupont", "Émilie Étoile", "Fanny Faure", "Zoé Zèbre"], names);
    }

    [Fact]
    public async Task Should_LeaveOutThePlayersOfAnotherTeam_When_ItIsTheSameSeason()
    {
        var board = GivenBoardMember();
        var marie = await GivenAnAdherentAsync(board, "Marie Adherente");
        var julie = await GivenAnAdherentAsync(board, "Julie Adherente");
        var first = await GivenTeamAsync(board, "D4");
        var second = await GivenTeamAsync(board, "D5");
        await As(board).PutAsync($"{V1}/interclubs/teams/{first}/members/{marie.Id}", null, CancellationToken);

        var candidates = await CandidatesAsync(board, second);

        Assert.DoesNotContain("Marie Adherente", candidates.Select(c => c.Name));
        Assert.Contains("Julie Adherente", candidates.Select(c => c.Name));
    }

    [Fact]
    public async Task Should_KeepThePlayersOfAnotherSeason_When_TheyDoNotPlayThisSeason()
    {
        var board = GivenBoardMember();
        var marie = await GivenAnAdherentAsync(board, "Marie Adherente");
        var lastSeason = await GivenSeasonAsync(board, "2025-09-01");
        var before = await GivenTeamAsync(board, "D4", lastSeason);
        var now = await GivenTeamAsync(board, "D4");
        await As(board).PutAsync($"{V1}/interclubs/teams/{before}/members/{marie.Id}", null, CancellationToken);

        var candidates = await CandidatesAsync(board, now);

        Assert.Contains("Marie Adherente", candidates.Select(c => c.Name));
    }

    [Fact]
    public async Task Should_LeaveOutTheMembersLicensedInAnotherClub_When_ThePlayersArePicked()
    {
        var board = GivenBoardMember();
        var marie = await GivenAnAdherentAsync(board, "Marie Adherente");
        var eric = await GivenAnExternalAdherentAsync(board, "Eric Externe");
        var teamId = await GivenTeamAsync(board);

        var candidates = await CandidatesAsync(board, teamId);

        Assert.Contains("Marie Adherente", candidates.Select(c => c.Name));
        Assert.DoesNotContain("Eric Externe", candidates.Select(c => c.Name));
        Assert.NotNull(marie);
        Assert.NotNull(eric);
    }
}
