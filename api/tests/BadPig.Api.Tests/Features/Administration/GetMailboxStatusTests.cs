using BadPig.Api.Tests.Infrastructure;

namespace BadPig.Api.Tests.Features.Administration;

public sealed class GetMailboxStatusTests(ApiFixture fixture) : IntegrationTest(fixture)
{
    private const string Url = $"{V1}/admin/mailbox";

    private async Task<MailboxDto> MailboxAsync(Person who) =>
        await ReadAsync<MailboxDto>(await As(who).GetAsync(Url, CancellationToken));

    [Fact]
    public async Task Should_ReturnUnauthorized_When_NobodyIsSignedIn()
    {
        var response = await Anonymous().GetAsync(Url, CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnForbidden_When_ThePersonIsNotOnTheBoard()
    {
        var member = GivenMember();

        var response = await As(member).GetAsync(Url, CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Should_CountTheUnreadMessages_When_TheClubMailboxCanBeReached()
    {
        var board = GivenBoardMember();
        GivenTheClubMailboxHas(3);

        var mailbox = await MailboxAsync(board);

        Assert.True(mailbox.IsAvailable);
        Assert.Equal(3, mailbox.UnreadCount);
        Assert.Equal("https://webmail.test.invalid", mailbox.WebmailUrl);
        Assert.NotNull(mailbox.CheckedAt);
    }

    [Fact]
    public async Task Should_ReportNothingToRead_When_EveryMessageHasBeenOpened()
    {
        var board = GivenBoardMember();
        GivenTheClubMailboxHas(0);

        var mailbox = await MailboxAsync(board);

        Assert.True(mailbox.IsAvailable);
        Assert.Equal(0, mailbox.UnreadCount);
    }

    [Fact]
    public async Task Should_ReportTheMailboxAsUnavailable_When_TheMailServerCannotBeReached()
    {
        var board = GivenBoardMember();
        // The substitute answers null by default: no mailbox configured, or the mail server did not answer.

        var mailbox = await MailboxAsync(board);

        Assert.False(mailbox.IsAvailable);
        Assert.Equal(0, mailbox.UnreadCount);
        Assert.Null(mailbox.WebmailUrl);
        Assert.Null(mailbox.CheckedAt);
    }
}
