using BadPig.Shared.Core;
using BadPig.Shared.Mailbox;
using BadPig.Shared.Payments;

namespace BadPig.Api.Tests.Infrastructure;

/// <summary>
/// Base of the integration tests: an empty database before each test, people to sign in with, and a few calls to the public API
/// that prepare a situation (a season, a title, a validated membership). Everything goes through HTTP, like the site does.
/// </summary>
[Collection(nameof(DatabaseTestCollection))]
public abstract class IntegrationTest(ApiFixture fixture) : IAsyncLifetime
{
    protected const string V1 = "/api/v1";

    protected TestApplication Api => fixture.Application;

    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync() => await fixture.ResetAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // --- People ---

    /// <summary>A person who has signed in at least once (the usual case).</summary>
    protected Person GivenMember(string name = "Marie Membre") => GivenAccount(name, ClubGroup.Members);

    protected Person GivenBoardMember(string name = "Paul Bureau") => GivenAccount(name, ClubGroup.Members, ClubGroup.Board);

    protected Person GivenAdmin(string name = "Alice Admin") => GivenAccount(name, ClubGroup.Members, ClubGroup.Board, ClubGroup.Admin);

    /// <summary>An invited person who has not completed the signup yet (never signed in).</summary>
    protected Person GivenInvitedPerson(string name = "Ines Invitee") => CreateAccount(name, false, true, ClubGroup.Members);

    /// <summary>A person invited into the board who has not completed the signup yet: their address is not confirmed.</summary>
    protected Person GivenInvitedBoardMember(string name = "Bruno Invite") => CreateAccount(name, false, true, ClubGroup.Members, ClubGroup.Board);

    protected Person GivenDisabledMember(string name = "Denis Desactive") => CreateAccount(name, true, false, ClubGroup.Members);

    /// <summary>A person who has signed in, in the given groups.</summary>
    protected Person GivenAccount(string name, params ClubGroup[] groups) => CreateAccount(name, true, true, groups);

    private Person CreateAccount(string name, bool hasSignedIn, bool isActive, params ClubGroup[] groups)
    {
        var id = Guid.NewGuid().ToString();
        var email = $"{name.ToLowerInvariant().Replace(' ', '.')}@example.test";
        var groupNames = groups.Select(Groups.NameOf).ToArray();
        Api.Accounts.Add(new AccountInfo(id, email, name, email, isActive, groupNames, hasSignedIn ? Api.Clock.UtcNow : null));
        return new Person(id, name, email, groupNames);
    }

    // --- Checks on the substitutes (what the application asked the outside world to do) ---

    protected void AssertEmailSentTo(Person person) =>
        _ = Api.Mail.Received().SendAsync(Arg.Is<AccountInfo?>(a => a != null && a.Id == person.Id), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

    protected void AssertNoEmailSent() =>
        _ = Api.Mail.DidNotReceiveWithAnyArgs().SendAsync(default, default!, default!, default);

    /// <summary>No "your order is paid" e-mail was sent (other e-mails, such as the validation of a membership in the set-up of the test, do not count).</summary>
    protected void AssertNoPaymentEmailSent() =>
        _ = Api.Mail.DidNotReceive().SendAsync(Arg.Any<AccountInfo?>(), "Votre commande est payée", Arg.Any<string>(), Arg.Any<CancellationToken>());

    protected void AssertSignupLinkSentTo(string accountId, int times) =>
        _ = Api.Identity.Received(times).SendSignupLinkAsync(accountId, Arg.Any<CancellationToken>());

    // --- Calls ---

    /// <summary>A client signed in as the person (null = nobody signed in).</summary>
    protected HttpClient As(Person? person)
    {
        var client = Api.CreateClient();
        if (person is null)
            return client;

        client.DefaultRequestHeaders.Add(TestAuthHandler.AccountHeader, person.Id);
        client.DefaultRequestHeaders.Add(TestAuthHandler.NameHeader, person.Name);
        client.DefaultRequestHeaders.Add(TestAuthHandler.GroupsHeader, string.Join(',', person.Groups));
        return client;
    }

    protected HttpClient Anonymous() => As(null);

    protected static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        var value = await response.Content.ReadFromJsonAsync<T>(CancellationToken);
        return value ?? throw new InvalidOperationException($"Empty answer ({(int)response.StatusCode}).");
    }

    // --- Announcements ---

    /// <summary>A member whose membership of the current season is validated: they can read the internal announcements.</summary>
    protected async Task<Person> GivenAnAdherentAsync(Person board, string name = "Marie Adherente")
    {
        // The season may already exist (a second adherent in the same test): a 409 is fine.
        await As(board).PostAsJsonAsync($"{V1}/admin/seasons", new { startsOn = "2026-09-01", internalFee = 60, externalFee = 30 }, CancellationToken);
        var member = GivenMember(name);
        await GivenValidatedMembershipAsync(board, member);
        return member;
    }

    /// <summary>Creates an announcement (a draft unless <paramref name="published"/>) and returns its identifier.</summary>
    protected async Task<string> GivenAnnouncementAsync(Person board, string title = "Tournoi interne", string kind = "internal",
        bool published = false, bool pinned = false, string bodyHtml = "<p>Rendez-vous au gymnase.</p>", bool? commentsEnabled = null)
    {
        var response = await As(board).PostAsJsonAsync($"{V1}/admin/announcements", new { title, bodyHtml, kind, commentsEnabled }, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = (await ReadAsync<IdDto>(response)).Id;
        if (published)
            Assert.Equal(HttpStatusCode.NoContent, (await As(board).PostAsync($"{V1}/admin/announcements/{id}/publish", null, CancellationToken)).StatusCode);

        if (pinned)
            Assert.Equal(HttpStatusCode.NoContent, (await As(board).PostAsync($"{V1}/admin/announcements/{id}/pin", null, CancellationToken)).StatusCode);

        return id;
    }

    protected async Task<AnnouncementDto> AnnouncementAsync(Person? reader, string id) =>
        await ReadAsync<AnnouncementDto>(await As(reader).GetAsync($"{V1}/announcements/{id}", CancellationToken));

    protected async Task<List<AnnouncementItemDto>> AnnouncementsAsync(Person? reader, string query = "") =>
        (await ReadAsync<PageDto<AnnouncementItemDto>>(await As(reader).GetAsync($"{V1}/announcements{query}", CancellationToken))).Items;

    // --- Polls ---

    /// <summary>
    /// Creates a poll (a draft unless <paramref name="published"/>) and returns its identifier. Default: "Oui" / "Non", single answer,
    /// not anonymous, open until 20 October 2026.
    /// </summary>
    protected async Task<string> GivenPollAsync(Person board, string[]? options = null, string title = "On se retrouve ?", string kind = "internal",
        bool published = false, bool allowMultiple = false, bool isAnonymous = false, string closesOn = "2026-10-20")
    {
        var response = await As(board).PostAsJsonAsync($"{V1}/admin/announcements/polls",
            new { title, kind, options = options ?? ["Oui", "Non"], allowMultiple, isAnonymous, closesOn }, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = (await ReadAsync<IdDto>(response)).Id;
        if (published)
            Assert.Equal(HttpStatusCode.NoContent, (await As(board).PostAsync($"{V1}/admin/announcements/{id}/publish", null, CancellationToken)).StatusCode);

        return id;
    }

    // --- Comments ---

    /// <summary>The person comments the announcement (or answers the comment <paramref name="parentId"/>) and the comment identifier is returned.</summary>
    protected async Task<string> GivenCommentAsync(Person who, string announcementId, string text = "Je viens !", string? parentId = null)
    {
        var response = await As(who).PostAsJsonAsync($"{V1}/announcements/{announcementId}/comments", new { text, parentId }, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadAsync<IdDto>(response)).Id;
    }

    protected async Task<List<CommentDto>> CommentsAsync(Person reader, string announcementId, string query = "") =>
        (await ReadAsync<PageDto<CommentDto>>(await As(reader).GetAsync($"{V1}/announcements/{announcementId}/comments{query}", CancellationToken))).Items;

    /// <summary>The person answers the poll with the options (by label).</summary>
    protected async Task GivenAnswerAsync(Person who, string announcementId, params string[] labels)
    {
        var poll = (await AnnouncementAsync(who, announcementId)).Poll!;
        var ids = labels.Select(label => poll.Options.Single(o => o.Label == label).Id).ToArray();
        var response = await As(who).PutAsJsonAsync($"{V1}/announcements/{announcementId}/poll/vote", new { optionIds = ids }, CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    protected static string OptionId(AnnouncementDto announcement, string label) => announcement.Poll!.Options.Single(o => o.Label == label).Id;

    /// <summary>Moves the clock to the day after the poll closes (21 October 2026 by default).</summary>
    protected void GivenThePollIsClosed() => Api.Clock.UtcNow.Returns(new DateTimeOffset(2026, 10, 21, 10, 0, 0, TimeSpan.Zero));

    /// <summary>The first bytes of a PNG file followed by some content: accepted as an image by the API.</summary>
    protected static byte[] PngBytes(int length = 64)
    {
        var bytes = new byte[length];
        new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }.CopyTo(bytes, 0);
        return bytes;
    }

    protected static MultipartFormDataContent FileForm(string fileName, byte[] bytes, string contentType = "application/octet-stream")
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { file, "file", fileName } };
    }

    // --- Profile pictures ---

    /// <summary>The person puts a profile picture (a small PNG) and its version is returned.</summary>
    protected async Task<long> GivenPhotoAsync(Person who, int length = 64)
    {
        var response = await As(who).PutAsync($"{V1}/me/photo", FileForm("moi.png", PngBytes(length), "image/png"), CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        return (await ReadAsync<ProfileDto>(await As(who).GetAsync($"{V1}/me", CancellationToken))).PhotoVersion!.Value;
    }

    // --- Usage statistics ---

    /// <summary>The person reports the use of a page (a view that stayed <paramref name="seconds"/> seconds, unless <paramref name="views"/> is 0).</summary>
    protected async Task GivenPageVisitAsync(Person who, string path, int seconds = 10, int views = 1)
    {
        var response = await As(who).PostAsJsonAsync($"{V1}/usage/visits", new { path, views, seconds }, CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    protected async Task<List<UsageItemDto>> UsageAsync(Person board, string query = "") =>
        (await ReadAsync<PageDto<UsageItemDto>>(await As(board).GetAsync($"{V1}/admin/usage{query}", CancellationToken))).Items;

    // --- Situations prepared through the API ---

    /// <summary>Creates a season (default: 2026-2027, fees 60 € for club licence holders and 30 € for external members) and returns its identifier.</summary>
    protected async Task<string> GivenSeasonAsync(Person board, string startsOn = "2026-09-01", decimal internalFee = 60, decimal externalFee = 30)
    {
        var response = await As(board).PostAsJsonAsync($"{V1}/admin/seasons", new { startsOn, internalFee, externalFee }, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadAsync<IdDto>(response)).Id;
    }

    /// <summary>Adds a period of school holidays to a season and returns its identifier.</summary>
    protected async Task<string> GivenHolidayAsync(Person board, string seasonId, string name = "Vacances de la Toussaint",
        string startsOn = "2026-10-17", string endsOn = "2026-11-02")
    {
        var response = await As(board).PostAsJsonAsync($"{V1}/admin/seasons/{seasonId}/holidays", new { name, startsOn, endsOn }, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadAsync<IdDto>(response)).Id;
    }

    /// <summary>Moves the clock to a day (Paris), at 10 in the morning.</summary>
    protected void GivenTodayIs(int year, int month, int day) =>
        Api.Clock.UtcNow.Returns(new DateTimeOffset(year, month, day, 10, 0, 0, TimeSpan.Zero));

    protected async Task<string> GivenTitleAsync(Person board, string label = "Président(e)")
    {
        var response = await As(board).PostAsJsonAsync($"{V1}/admin/titles", new { label }, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadAsync<IdDto>(response)).Id;
    }

    /// <summary>A member whose membership is validated as "external": they are licensed in another club.</summary>
    protected async Task<Person> GivenAnExternalAdherentAsync(Person board, string name = "Eric Externe")
    {
        await As(board).PostAsJsonAsync($"{V1}/admin/seasons", new { startsOn = "2026-09-01", internalFee = 60, externalFee = 30 }, CancellationToken);
        var member = GivenMember(name);
        await GivenValidatedMembershipAsync(board, member, "external");
        return member;
    }

    protected async Task GivenValidatedMembershipAsync(Person board, Person member, string? kind = null)
    {
        var response = await As(board).PostAsJsonAsync($"{V1}/admin/users/{member.Id}/membership/validate", new { kind }, CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    protected async Task<UserDto> UserAsync(Person board, Person person)
    {
        var page = await ReadAsync<PageDto<UserDto>>(await As(board).GetAsync($"{V1}/admin/users?pageSize=100", CancellationToken));
        return page.Items.Single(u => u.Id == person.Id);
    }

    // --- Shop ---

    /// <summary>Creates an article and returns its identifier.</summary>
    protected async Task<string> GivenProductAsync(Person board, string name = "Tube de volants", string? description = null)
    {
        var response = await As(board).PostAsJsonAsync($"{V1}/admin/shop/products", new { name, description }, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadAsync<IdDto>(response)).Id;
    }

    /// <summary>Adds a variant to the article and returns its identifier.</summary>
    protected async Task<string> GivenVariantAsync(Person board, string productId, string label = "Vitesse 77", int priceCents = 2000, int quantity = 10, int? lowStockThreshold = null)
    {
        var response = await As(board).PostAsJsonAsync($"{V1}/admin/shop/products/{productId}/variants", new { label, priceCents, quantity, lowStockThreshold }, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadAsync<IdDto>(response)).Id;
    }

    /// <summary>An article with one variant (a tube of shuttlecocks "Vitesse 77", 20 €, 10 in stock by default); returns the variant identifier.</summary>
    protected async Task<string> GivenAnArticleAsync(Person board, string name = "Tube de volants", string label = "Vitesse 77", int priceCents = 2000, int quantity = 10, int? lowStockThreshold = null) =>
        await GivenVariantAsync(board, await GivenProductAsync(board, name), label, priceCents, quantity, lowStockThreshold);

    /// <summary>The person orders the variants (identifier, quantity) and the order identifier is returned.</summary>
    protected async Task<string> GivenOrderAsync(Person who, params (string VariantId, int Quantity)[] lines)
    {
        var response = await As(who).PostAsJsonAsync($"{V1}/shop/orders", new { lines = lines.Select(l => new { variantId = l.VariantId, quantity = l.Quantity }) }, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadAsync<IdDto>(response)).Id;
    }

    protected async Task<OrderDto> OrderAsync(Person who, string id) =>
        await ReadAsync<OrderDto>(await As(who).GetAsync($"{V1}/shop/orders/{id}", CancellationToken));

    protected async Task<List<ProductItemDto>> ProductsAsync(Person board, string query = "") =>
        (await ReadAsync<PageDto<ProductItemDto>>(await As(board).GetAsync($"{V1}/admin/shop/products{query}", CancellationToken))).Items;

    protected async Task<ProductVariantDto> VariantAsync(Person board, string variantId) =>
        (await ProductsAsync(board, "?pageSize=100")).SelectMany(p => p.Variants).Single(v => v.Id == variantId);

    protected async Task<List<CatalogItemDto>> CatalogAsync(Person who) =>
        (await ReadAsync<PageDto<CatalogItemDto>>(await As(who).GetAsync($"{V1}/shop/products?pageSize=100", CancellationToken))).Items;

    /// <summary>The payment provider says that the payment page of the order is now in this state (the buyer comes back, or the order is read).</summary>
    protected async Task GivenThePaymentPageIsAsync(Person buyer, string orderId, PaymentState state)
    {
        var sessionId = SessionIdOf(await OrderAsync(buyer, orderId));
        Api.Payments.GetSessionStateAsync(sessionId, Arg.Any<CancellationToken>()).Returns(Task.FromResult<PaymentState?>(state));
    }

    /// <summary>The order is paid: the payment provider says so and the buyer reads the order.</summary>
    protected async Task GivenTheOrderIsPaidAsync(Person buyer, string orderId)
    {
        await GivenThePaymentPageIsAsync(buyer, orderId, PaymentState.Paid);
        Assert.Equal("paid", (await OrderAsync(buyer, orderId)).Status);
    }

    /// <summary>The identifier of the payment page, at the end of the payment address.</summary>
    protected static string SessionIdOf(OrderDto order) => order.CheckoutUrl![(order.CheckoutUrl.LastIndexOf('/') + 1)..];

    /// <summary>The next notification received by the API is a valid one from the payment provider about this payment page.</summary>
    protected void GivenThePaymentProviderNotifies(string sessionId, PaymentState state) =>
        Api.Payments.ReadNotification(Arg.Any<string>(), Arg.Any<string?>())
            .Returns(Result<PaymentNotification?>.Success(new PaymentNotification(sessionId, state)));

    protected async Task<HttpResponseMessage> PostNotificationAsync() =>
        await Anonymous().PostAsync($"{V1}/shop/payments/notifications", new StringContent("{\"type\":\"test\"}", System.Text.Encoding.UTF8, "application/json"), CancellationToken);

    // --- Interclubs ---

    /// <summary>The board creates a team (D4 of the season 2026-2027 by default), with a captain when given, and its identifier is returned.</summary>
    /// <summary>The season of the club used by the interclub tests (2026-2027): created if it is not there yet, and its identifier returned.</summary>
    protected async Task<string> GivenTheCurrentSeasonAsync(Person board)
    {
        var seasons = (await ReadAsync<PageDto<SeasonDto>>(await As(board).GetAsync($"{V1}/admin/seasons", CancellationToken))).Items;
        return seasons.FirstOrDefault(s => s.Name == "2026-2027")?.Id ?? await GivenSeasonAsync(board);
    }

    /// <summary>Creates a team in a season (the current one by default) and returns its identifier.</summary>
    protected async Task<string> GivenTeamAsync(Person board, string name = "D4", string? seasonId = null, Person? captain = null)
    {
        seasonId ??= await GivenTheCurrentSeasonAsync(board);
        var response = await As(board).PostAsJsonAsync($"{V1}/admin/interclubs/teams", new { name, seasonId }, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var id = (await ReadAsync<IdDto>(response)).Id;
        if (captain is not null)
            Assert.Equal(HttpStatusCode.NoContent, (await As(board).PutAsJsonAsync($"{V1}/admin/interclubs/teams/{id}/captain", new { captainId = captain.Id }, CancellationToken)).StatusCode);

        return id;
    }

    /// <summary>The person (captain or board) adds a fixture to the calendar of the team and its identifier is returned.</summary>
    protected async Task<string> GivenFixtureAsync(Person who, string teamId, string date = "2026-11-14", string opponent = "BC Lyon", string venue = "home",
        string? startTime = null, string? place = null)
    {
        var response = await As(who).PostAsJsonAsync($"{V1}/interclubs/teams/{teamId}/fixtures", new { date, startTime, opponent, venue, place }, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadAsync<IdDto>(response)).Id;
    }

    protected async Task<List<TeamItemDto>> TeamsAsync(Person reader, string query = "") =>
        (await ReadAsync<PageDto<TeamItemDto>>(await As(reader).GetAsync($"{V1}/interclubs/teams{query}", CancellationToken))).Items;

    protected async Task<List<FixtureItemDto>> FixturesAsync(Person reader, string query = "") =>
        (await ReadAsync<PageDto<FixtureItemDto>>(await As(reader).GetAsync($"{V1}/interclubs/fixtures{query}", CancellationToken))).Items;

    protected async Task<FixtureDto> FixtureAsync(Person reader, string id) =>
        await ReadAsync<FixtureDto>(await As(reader).GetAsync($"{V1}/interclubs/fixtures/{id}", CancellationToken));

    /// <summary>The manager (captain or board) adds the person to the players of the team.</summary>
    protected async Task GivenPlayerAsync(Person manager, string teamId, Person player) =>
        Assert.Equal(HttpStatusCode.NoContent, (await As(manager).PutAsync($"{V1}/interclubs/teams/{teamId}/members/{player.Id}", null, CancellationToken)).StatusCode);

    /// <summary>The person (captain or board) offers two dates (14 and 21 November 2026 by default) for the next fixture and the proposal identifier is returned.</summary>
    protected async Task<string> GivenProposalAsync(Person who, string teamId, string opponent = "BC Lyon", string venue = "home", params string[] dates)
    {
        var offered = (dates.Length > 0 ? dates : ["2026-11-14", "2026-11-21"]).Select(date => new { date, startTime = (string?)null }).ToArray();
        var response = await As(who).PostAsJsonAsync($"{V1}/interclubs/teams/{teamId}/proposals", new { opponent, venue, dates = offered }, CancellationToken);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await ReadAsync<IdDto>(response)).Id;
    }

    protected async Task<ProposalDto> ProposalAsync(Person reader, string id) =>
        await ReadAsync<ProposalDto>(await As(reader).GetAsync($"{V1}/interclubs/proposals/{id}", CancellationToken));

    /// <summary>The person answers the date (by its position in the proposal, from the earliest) of the proposal.</summary>
    protected async Task GivenAnswerAsync(Person who, string proposalId, int dateIndex, string answer)
    {
        var proposal = await ProposalAsync(who, proposalId);
        var response = await As(who).PutAsJsonAsync($"{V1}/interclubs/proposals/{proposalId}/answers",
            new { answers = new[] { new { dateId = proposal.Dates[dateIndex].Id, answer } } }, CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    protected static readonly string[] MatchSlotCodes = ["SD", "MX1", "MX2", "DD", "DH", "SH1", "SH2"];

    /// <summary>The body of a result where the matches in <paramref name="won"/> are won 2-0 by the club (15-10, 15-8) and the others lost 0-2 (9-15, 12-15).</summary>
    protected static object ResultBody(params string[] won) =>
        new
        {
            matches = MatchSlotCodes.Select(slot => new
            {
                slot,
                sets = won.Contains(slot)
                    ? new[] { new { club = 15, opponent = 10 }, new { club = 15, opponent = 8 } }
                    : new[] { new { club = 9, opponent = 15 }, new { club = 12, opponent = 15 } },
            }).ToArray(),
        };

    /// <summary>The captain (or the board) enters one match of a fixture, won by the club in two sets unless <paramref name="won"/> says otherwise.</summary>
    protected async Task GivenMatchResultAsync(Person who, string fixtureId, string slot, bool won = true)
    {
        var sets = won
            ? new[] { new { club = 15, opponent = 10 }, new { club = 15, opponent = 8 } }
            : [new { club = 9, opponent = 15 }, new { club = 12, opponent = 15 }];
        var response = await As(who).PutAsJsonAsync($"{V1}/interclubs/fixtures/{fixtureId}/matches/{slot}/result", new { sets }, CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    /// <summary>The captain (or the board) enters a result of the fixture (see <see cref="ResultBody"/>).</summary>
    protected async Task GivenResultAsync(Person who, string fixtureId, params string[] won) =>
        Assert.Equal(HttpStatusCode.NoContent, (await As(who).PutAsJsonAsync($"{V1}/interclubs/fixtures/{fixtureId}/result", ResultBody(won), CancellationToken)).StatusCode);

    // --- Club mailbox ---

    /// <summary>A club mailbox that can be reached and holds this many unread messages.</summary>
    protected void GivenTheClubMailboxHas(int unread)
    {
        // The time is read before Returns(): reading a substitute inside it would configure the clock instead.
        var now = Api.Clock.UtcNow;
        Api.Mailbox.ReadStatusAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<MailboxStatus?>(new MailboxStatus(unread, now)));
    }
}
