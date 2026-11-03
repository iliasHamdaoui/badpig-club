using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Profile;

public sealed class ListClubDirectoryTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Url = $"{V1}/club/directory";

    private async Task<List<DirectoryItemDto>> DirectoryAsync(Person reader, string query = "") =>
        (await ReadAsync<PageDto<DirectoryItemDto>>(await As(reader).GetAsync(Url + query, CancellationToken))).Items;

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync(Url, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_TheMembershipIsNotValidated()
    {
        var response = await As(GivenMember()).GetAsync(Url, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ListOnlyThePeopleWithAPicture_When_AnAdherentAsks()
    {
        var board = GivenBoardMember();
        var withPicture = await GivenAnAdherentAsync(board, "Marie Adherente");
        await GivenAnAdherentAsync(board, "Jean Sansphoto");
        var version = await GivenPhotoAsync(withPicture);

        var items = await DirectoryAsync(withPicture);

        var item = Assert.Single(items);
        Assert.Equal(withPicture.Id, item.Id);
        Assert.Equal("Marie Adherente", item.Name);
        Assert.Equal(version, item.PhotoVersion);
    }

    [Fact]
    public async Task Should_SortByNameWithoutAccents_When_SeveralPeopleHaveAPicture()
    {
        var board = GivenBoardMember();
        var zoe = await GivenAnAdherentAsync(board, "Zoé Martin");
        var emilie = await GivenAnAdherentAsync(board, "Émilie Durand");
        await GivenPhotoAsync(zoe);
        await GivenPhotoAsync(emilie);

        var items = await DirectoryAsync(board);

        Assert.Equal(["Émilie Durand", "Zoé Martin"], items.Select(i => i.Name).ToArray());
    }

    [Fact]
    public async Task Should_FilterByName_When_ISearch()
    {
        var board = GivenBoardMember();
        var marie = await GivenAnAdherentAsync(board, "Marie Adherente");
        var jean = await GivenAnAdherentAsync(board, "Jean Dupont");
        await GivenPhotoAsync(marie);
        await GivenPhotoAsync(jean);

        var items = await DirectoryAsync(board, "?search=dupont");

        Assert.Equal("Jean Dupont", Assert.Single(items).Name);
    }

    [Fact]
    public async Task Should_HideDisabledAccounts_When_ListingTheDirectory()
    {
        var board = GivenBoardMember();
        var member = await GivenAnAdherentAsync(board);
        await GivenPhotoAsync(member);
        await As(board).PostAsync($"{V1}/admin/users/{member.Id}/disable", null, CancellationToken);

        var items = await DirectoryAsync(board);

        Assert.Empty(items);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheSortIsUnknown()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{Url}?sort=inconnu", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
