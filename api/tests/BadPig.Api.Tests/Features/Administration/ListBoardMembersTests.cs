using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class ListBoardMembersTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private string Url(string query = "") => $"{V1}/club/board{query}";

    private async Task<List<BoardMemberDto>> BoardAsync(Person reader, string query = "") =>
        (await ReadAsync<PageDto<BoardMemberDto>>(await As(reader).GetAsync(Url(query), CancellationToken))).Items;

    private async Task GivenTitleOfAsync(Person board, Person holder, string label)
    {
        var titleId = await GivenTitleAsync(board, label);
        var response = await As(board).PutAsJsonAsync($"{V1}/admin/users/{holder.Id}/title", new { titleId }, CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync(Url(), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_ThePersonIsNotAClubMember()
    {
        var outsider = GivenAccount("Olivier Exterieur");

        var response = await As(outsider).GetAsync(Url(), CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ListTheBoardWithItsTitlesAndEmails_When_AnAdherentAsks()
    {
        var board = GivenBoardMember("Savinien Ferrant");
        var treasurer = GivenBoardMember("Camille Roux");
        GivenMember("Marie Membre");
        await GivenTitleOfAsync(board, board, "Président");
        await GivenTitleOfAsync(board, treasurer, "Trésorière");
        var adherent = await GivenAnAdherentAsync(board);

        var people = await BoardAsync(adherent);

        Assert.Equal(["Camille Roux", "Savinien Ferrant"], people.Select(p => p.Name));
        Assert.Equal(["Trésorière", "Président"], people.Select(p => p.Title));
        Assert.Equal(treasurer.Email, people[0].Email);
        Assert.Equal(treasurer.Id, people[0].AccountId);
    }

    [Fact]
    public async Task Should_LeaveOutTheMembersAndTheDisabledAccounts_When_TheBoardIsListed()
    {
        var board = GivenBoardMember();
        GivenMember("Marie Membre");
        GivenDisabledMember("Denis Desactive");
        var adherent = await GivenAnAdherentAsync(board);

        var people = await BoardAsync(adherent);

        Assert.Equal([board.Name], people.Select(p => p.Name));
    }

    [Fact]
    public async Task Should_GiveNoTitle_When_ThePersonHoldsNone()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);

        Assert.Null(Assert.Single(await BoardAsync(adherent)).Title);
    }

    [Fact]
    public async Task Should_SortOnTheTitle_When_AskedTo()
    {
        var board = GivenBoardMember("Savinien Ferrant");
        var secretary = GivenBoardMember("Anna Secretaire");
        await GivenTitleOfAsync(board, board, "Président");
        await GivenTitleOfAsync(board, secretary, "Secrétaire");
        var adherent = await GivenAnAdherentAsync(board);

        var withoutTitle = GivenBoardMember("Hugo Perret");

        var people = await BoardAsync(adherent, "?sort=title");

        // Those who hold a title first, then the others.
        Assert.Equal(["Président", "Secrétaire", null], people.Select(p => p.Title));
        Assert.Equal(withoutTitle.Name, people[2].Name);
    }

    [Fact]
    public async Task Should_FindAPerson_When_TheSearchMatchesTheNameOrTheTitle()
    {
        var board = GivenBoardMember("Savinien Ferrant");
        var other = GivenBoardMember("Camille Roux");
        await GivenTitleOfAsync(board, other, "Trésorière");
        var adherent = await GivenAnAdherentAsync(board);

        Assert.Equal("Camille Roux", Assert.Single(await BoardAsync(adherent, "?search=roux")).Name);
        Assert.Equal("Camille Roux", Assert.Single(await BoardAsync(adherent, "?search=tr%C3%A9so")).Name);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_SortIsUnknown()
    {
        var board = GivenBoardMember();
        var adherent = await GivenAnAdherentAsync(board);

        var response = await As(adherent).GetAsync(Url("?sort=email"), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
