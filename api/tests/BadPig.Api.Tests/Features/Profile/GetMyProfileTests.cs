using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Profile;

public sealed class GetMyProfileTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync($"{V1}/me", CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnMyProfile_When_IAmSignedIn()
    {
        var me = GivenMember("Marie Membre");

        var response = await As(me).GetAsync($"{V1}/me", CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await ReadAsync<ProfileDto>(response);
        Assert.Equal(me.Id, profile.Id);
        Assert.Equal("Marie Membre", profile.Name);
        Assert.Equal(me.Email, profile.Email);
        Assert.Contains("Membres", profile.Groups);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When_MyAccountNoLongerExistsInTheIdentityProvider()
    {
        var me = GivenMember();
        Api.Accounts.Clear();

        var response = await As(me).GetAsync($"{V1}/me", CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReportMembershipToValidate_When_BoardHasNotValidatedMyMembership()
    {
        var board = GivenBoardMember();
        var me = GivenMember();
        await GivenSeasonAsync(board);

        var profile = await ReadAsync<ProfileDto>(await As(me).GetAsync($"{V1}/me", CancellationToken));

        Assert.Equal("to_validate", profile.Membership);
        Assert.False(profile.MembershipUpToDate);
    }

    [Fact]
    public async Task Should_ReportMembershipValid_When_BoardHasValidatedMyMembership()
    {
        var board = GivenBoardMember();
        var me = GivenMember();
        await GivenSeasonAsync(board);
        await GivenValidatedMembershipAsync(board, me);

        var profile = await ReadAsync<ProfileDto>(await As(me).GetAsync($"{V1}/me", CancellationToken));

        Assert.Equal("valid", profile.Membership);
        Assert.True(profile.MembershipUpToDate);
    }

    [Fact]
    public async Task Should_KeepAccessToTheMembersArea_When_IAmOnTheBoardWithoutValidatedMembership()
    {
        var board = GivenBoardMember();
        await GivenSeasonAsync(board);

        var profile = await ReadAsync<ProfileDto>(await As(board).GetAsync($"{V1}/me", CancellationToken));

        Assert.True(profile.MembershipUpToDate);
    }

    [Fact]
    public async Task Should_ReturnMyTitle_When_BoardHasGivenMeOne()
    {
        var board = GivenBoardMember();
        var titleId = await GivenTitleAsync(board, "Trésorier(e)");
        await As(GivenAdmin()).PutAsJsonAsync($"{V1}/admin/users/{board.Id}/title", new { titleId }, CancellationToken);

        var profile = await ReadAsync<ProfileDto>(await As(board).GetAsync($"{V1}/me", CancellationToken));

        Assert.Equal("Trésorier(e)", profile.Title);
    }

    [Fact]
    public async Task Should_TellSinceWhenIAmAMember_When_MyMembershipHasBeenValidated()
    {
        var board = GivenBoardMember();
        var me = GivenMember();
        await GivenSeasonAsync(board, "2025-09-01");
        GivenTodayIs(2025, 9, 20);
        await GivenValidatedMembershipAsync(board, me);
        var firstDay = Api.Clock.UtcNow;
        await GivenSeasonAsync(board);
        GivenTodayIs(2026, 10, 15);
        await GivenValidatedMembershipAsync(board, me);

        var profile = await ReadAsync<ProfileDto>(await As(me).GetAsync($"{V1}/me", CancellationToken));

        Assert.Equal(firstDay, profile.MemberSince);
    }

    [Fact]
    public async Task Should_TellNoMembershipDateAndNoLicence_When_NothingWasRecordedYet()
    {
        var board = GivenBoardMember();
        var me = GivenMember();
        await GivenSeasonAsync(board);

        var profile = await ReadAsync<ProfileDto>(await As(me).GetAsync($"{V1}/me", CancellationToken));

        Assert.Null(profile.MemberSince);
        Assert.Null(profile.Licence);
    }
}
