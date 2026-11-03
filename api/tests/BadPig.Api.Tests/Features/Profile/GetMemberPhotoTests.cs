using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Profile;

public sealed class GetMemberPhotoTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync($"{V1}/members/{Guid.NewGuid()}/photo", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnThePicture_When_AnAdherentAsksForIt()
    {
        var board = GivenBoardMember();
        var owner = await GivenAnAdherentAsync(board, "Marie Adherente");
        var reader = await GivenAnAdherentAsync(board, "Jean Adherent");
        await GivenPhotoAsync(owner, 64);

        var response = await As(reader).GetAsync($"{V1}/members/{owner.Id}/photo", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(64, (await response.Content.ReadAsByteArrayAsync(CancellationToken)).Length);
    }

    [Fact]
    public async Task Should_ReturnThePicture_When_TheBoardAsksForIt()
    {
        var owner = GivenMember();
        await GivenPhotoAsync(owner);

        var response = await As(GivenBoardMember()).GetAsync($"{V1}/members/{owner.Id}/photo", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_TheMembershipIsNotValidated()
    {
        var board = GivenBoardMember();
        var owner = await GivenAnAdherentAsync(board);
        await GivenPhotoAsync(owner);

        var response = await As(GivenMember("Nouveau Membre")).GetAsync($"{V1}/members/{owner.Id}/photo", CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_ThePersonHasNoPicture()
    {
        var board = GivenBoardMember();
        var owner = await GivenAnAdherentAsync(board);

        var response = await As(board).GetAsync($"{V1}/members/{owner.Id}/photo", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheIdentifierIsInvalid()
    {
        var response = await As(GivenBoardMember()).GetAsync($"{V1}/members/pas-un-guid/photo", CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ErasePicture_When_TheOwnerDeletesTheirAccount()
    {
        var board = GivenBoardMember();
        var owner = await GivenAnAdherentAsync(board);
        await GivenPhotoAsync(owner);

        await As(owner).DeleteAsync($"{V1}/me", CancellationToken);

        var directory = await ReadAsync<PageDto<DirectoryItemDto>>(await As(board).GetAsync($"{V1}/club/directory", CancellationToken));
        Assert.Empty(directory.Items);
    }
}
