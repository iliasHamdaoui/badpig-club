using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Profile;

public sealed class UpdateMyProfileTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().PutAsJsonAsync($"{V1}/me", new { name = "X", email = "x@example.test" }, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ChangeMyNameAndEmail_When_DataIsValid()
    {
        var me = GivenMember();

        var response = await As(me).PutAsJsonAsync($"{V1}/me", new { name = "Marie Nouvelle", email = "marie.nouvelle@example.test" }, CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var profile = await ReadAsync<ProfileDto>(await As(me).GetAsync($"{V1}/me", CancellationToken));
        Assert.Equal("Marie Nouvelle", profile.Name);
        Assert.Equal("marie.nouvelle@example.test", profile.Email);
    }

    [Fact]
    public async Task Should_ReturnBadRequestOnTheEmailField_When_EmailIsInvalid()
    {
        var me = GivenMember();

        var response = await As(me).PutAsJsonAsync($"{V1}/me", new { name = "Marie", email = "not-an-email" }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(CancellationToken);
        Assert.True(problem.GetProperty("errors").TryGetProperty("email", out _));
    }

    [Fact]
    public async Task Should_ReturnBadRequestOnTheNameField_When_NameIsEmpty()
    {
        var me = GivenMember();

        var response = await As(me).PutAsJsonAsync($"{V1}/me", new { name = "", email = me.Email }, CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(CancellationToken);
        Assert.True(problem.GetProperty("errors").TryGetProperty("name", out _));
    }

    [Fact]
    public async Task Should_NotChangeAnotherPerson_When_ISendMyOwnProfile()
    {
        var me = GivenMember("Marie Membre");
        var other = GivenMember("Olivier Autre");

        await As(me).PutAsJsonAsync($"{V1}/me", new { name = "Marie Nouvelle", email = me.Email }, CancellationToken);

        Assert.Equal("Olivier Autre", Api.Accounts.Get(other.Id)!.Name);
    }
}
