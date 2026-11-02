using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Profile;

public sealed class SetMyLicenceTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Url = $"{V1}/me/licence";

    private async Task<ProfileDto> ProfileAsync(Person me) =>
        await ReadAsync<ProfileDto>(await As(me).GetAsync($"{V1}/me", CancellationToken));

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync(Url, new { number = "07123456" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnBadRequest_When_TheNumberIsNotALicenceNumber()
    {
        var me = GivenMember();

        var tooLong = await As(me).PutAsJsonAsync(Url, new { number = new string('1', 21) }, CancellationToken);
        var wrongCharacters = await As(me).PutAsJsonAsync(Url, new { number = "07 123/456" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, tooLong.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, wrongCharacters.StatusCode);
    }

    [Fact]
    public async Task Should_RecordMyLicence_When_IEnterIt()
    {
        var me = GivenMember();

        var response = await As(me).PutAsJsonAsync(Url, new { number = " 07123456 " }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("07123456", (await ProfileAsync(me)).Licence);
    }

    [Fact]
    public async Task Should_ReplaceMyLicence_When_IEnterAnotherOne()
    {
        var me = GivenMember();
        await As(me).PutAsJsonAsync(Url, new { number = "07123456" }, CancellationToken);

        await As(me).PutAsJsonAsync(Url, new { number = "07999999" }, CancellationToken);

        Assert.Equal("07999999", (await ProfileAsync(me)).Licence);
    }

    [Fact]
    public async Task Should_RemoveMyLicence_When_IClearIt()
    {
        var me = GivenMember();
        await As(me).PutAsJsonAsync(Url, new { number = "07123456" }, CancellationToken);

        var response = await As(me).PutAsJsonAsync(Url, new { number = "" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null((await ProfileAsync(me)).Licence);
    }

    [Fact]
    public async Task Should_KeepTheLicencesApart_When_SeveralPeopleEnterOne()
    {
        var me = GivenMember();
        var other = GivenMember("Autre Adherent");
        await As(me).PutAsJsonAsync(Url, new { number = "07123456" }, CancellationToken);

        await As(other).PutAsJsonAsync(Url, new { number = "07999999" }, CancellationToken);

        Assert.Equal("07123456", (await ProfileAsync(me)).Licence);
        Assert.Equal("07999999", (await ProfileAsync(other)).Licence);
    }

    [Fact]
    public async Task Should_ReturnConflict_When_AnotherMemberAlreadyHasThatLicence()
    {
        var marie = GivenMember("Marie Membre");
        var zoe = GivenMember("Zoe Membre");
        Assert.Equal(HttpStatusCode.NoContent, (await As(marie).PutAsJsonAsync(Url, new { number = "07123456" }, CancellationToken)).StatusCode);

        var response = await As(zoe).PutAsJsonAsync(Url, new { number = "07123456" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Null((await ProfileAsync(zoe)).Licence);
    }

    [Fact]
    public async Task Should_KeepTheLicence_When_ISaveMyOwnNumberAgain()
    {
        var me = GivenMember();
        await As(me).PutAsJsonAsync(Url, new { number = "07123456" }, CancellationToken);

        var response = await As(me).PutAsJsonAsync(Url, new { number = "07123456" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("07123456", (await ProfileAsync(me)).Licence);
    }
}
