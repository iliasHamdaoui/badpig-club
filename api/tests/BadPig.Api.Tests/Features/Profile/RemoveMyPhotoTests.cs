using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Profile;

public sealed class RemoveMyPhotoTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Url = $"{V1}/me/photo";

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().DeleteAsync(Url, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_RemoveMyPhoto_When_IHaveOne()
    {
        var me = GivenMember();
        await GivenPhotoAsync(me);

        var response = await As(me).DeleteAsync(Url, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null((await ReadAsync<ProfileDto>(await As(me).GetAsync($"{V1}/me", CancellationToken))).PhotoVersion);
    }

    [Fact]
    public async Task Should_NotFail_When_IHaveNoPhoto()
    {
        var response = await As(GivenMember()).DeleteAsync(Url, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Should_KeepTheOthersPhotos_When_IRemoveMine()
    {
        var board = GivenBoardMember();
        var me = await GivenAnAdherentAsync(board, "Marie Adherente");
        var other = await GivenAnAdherentAsync(board, "Jean Adherent");
        await GivenPhotoAsync(me);
        await GivenPhotoAsync(other);

        await As(me).DeleteAsync(Url, CancellationToken);

        var response = await As(board).GetAsync($"{V1}/members/{other.Id}/photo", CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
