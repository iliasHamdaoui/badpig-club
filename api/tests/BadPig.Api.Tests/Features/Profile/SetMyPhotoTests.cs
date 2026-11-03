using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Profile;

public sealed class SetMyPhotoTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Url = $"{V1}/me/photo";

    private async Task<ProfileDto> ProfileAsync(Person me) =>
        await ReadAsync<ProfileDto>(await As(me).GetAsync($"{V1}/me", CancellationToken));

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsync(Url, FileForm("moi.png", PngBytes(), "image/png"), CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_SetMyPhoto_When_IUploadAnImage()
    {
        var me = GivenMember();

        var response = await As(me).PutAsync(Url, FileForm("moi.png", PngBytes(), "image/png"), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull((await ProfileAsync(me)).PhotoVersion);
    }

    [Fact]
    public async Task Should_ReplaceMyPhoto_When_IUploadAnotherOne()
    {
        var me = GivenMember();
        await GivenPhotoAsync(me, 64);

        var response = await As(me).PutAsync(Url, FileForm("autre.png", PngBytes(128), "image/png"), CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotNull((await ProfileAsync(me)).PhotoVersion);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_NoFileIsSent()
    {
        var response = await As(GivenMember()).PutAsync(Url, new MultipartFormDataContent(), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheFileIsNotAnImage()
    {
        var response = await As(GivenMember()).PutAsync(Url, FileForm("faux.png", System.Text.Encoding.UTF8.GetBytes("<html>not an image</html>"), "image/png"), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheExtensionIsNotAnImageOne()
    {
        var response = await As(GivenMember()).PutAsync(Url, FileForm("moi.gif", PngBytes()), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheImageIsTooLarge()
    {
        var response = await As(GivenMember()).PutAsync(Url, FileForm("grande.png", PngBytes(1024 * 1024 + 1), "image/png"), CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
