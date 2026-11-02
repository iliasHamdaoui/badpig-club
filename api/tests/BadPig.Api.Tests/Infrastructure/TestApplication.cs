using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Mailbox;
using BadPig.Shared.Payments;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute.ClearExtensions;

namespace BadPig.Api.Tests.Infrastructure;

/// <summary>
/// The real application, in memory, on the test database. Only what lives outside of the application is replaced:
/// the identity provider (Authentik), the e-mails, the clock and the token check (the tests sign people in by headers).
/// </summary>
public sealed class TestApplication(string connectionString) : WebApplicationFactory<Program>
{
    // Folder of the uploaded files (posters, attachments) of this test run.
    private readonly string _storagePath = Path.Combine(Path.GetTempPath(), "badpig-api-tests", Guid.NewGuid().ToString("N"));

    /// <summary>Folder where the API stores the uploaded files during this test run.</summary>
    public string StoragePath => _storagePath;

    private static readonly DateTimeOffset DefaultNow = new(2026, 10, 15, 10, 0, 0, TimeSpan.Zero);

    /// <summary>The accounts of the identity provider (the situation of a test).</summary>
    public AccountDirectory Accounts { get; } = new();

    /// <summary>Substitute of the identity provider (Authentik): check the calls with <c>Received()</c>, change an answer with <c>Returns()</c>.</summary>
    public IIdentityProvider Identity { get; } = Substitute.For<IIdentityProvider>();

    /// <summary>Substitute of the e-mail sender.</summary>
    public IEmailSender Mail { get; } = Substitute.For<IEmailSender>();

    /// <summary>Substitute of the clock: 15 October 2026 by default, in the middle of the 2026-2027 season.</summary>
    public IDateTimeProvider Clock { get; } = Substitute.For<IDateTimeProvider>();

    /// <summary>Substitute of the payment provider (Stripe): by default it opens payment pages that stay open; check the calls with <c>Received()</c>.</summary>
    public IPaymentProvider Payments { get; } = Substitute.For<IPaymentProvider>();

    /// <summary>Substitute of the club mailbox: no mailbox configured by default (see GivenTheClubMailboxHas).</summary>
    public IMailboxReader Mailbox { get; } = Substitute.For<IMailboxReader>();

    public TestApplication Prepare()
    {
        Reset();
        return this;
    }

    /// <summary>Back to the default situation: no account, no recorded call, default answers and default time.</summary>
    public void Reset()
    {
        Accounts.Clear();
        if (Directory.Exists(_storagePath))
            Directory.Delete(_storagePath, recursive: true);

        Identity.ClearSubstitute(ClearOptions.All);
        Mail.ClearSubstitute(ClearOptions.All);
        Clock.ClearSubstitute(ClearOptions.All);
        Payments.ClearSubstitute(ClearOptions.All);
        Mailbox.ClearSubstitute(ClearOptions.All);
        Accounts.Wire(Identity);
        Clock.UtcNow.Returns(DefaultNow);
        WirePayments();
        // No mailbox configured by default: the site shows nothing (see GivenTheClubMailboxHas).
        Mailbox.ReadStatusAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<MailboxStatus?>(null));
    }

    // A payment page per order, still open; a notification is refused unless a test says otherwise (see GivenThePaymentProviderNotifies).
    private void WirePayments()
    {
        Payments.CreateSessionAsync(Arg.Any<PaymentRequest>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            var id = $"cs_test_{Guid.NewGuid():N}";
            return Task.FromResult<Result<PaymentSession>>(new PaymentSession(id, $"https://checkout.test/pay/{id}"));
        });
        Payments.GetSessionStateAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult<PaymentState?>(PaymentState.Open));
        Payments.ReadNotification(Arg.Any<string>(), Arg.Any<string?>())
            .Returns(Result<PaymentNotification?>.Failure(Error.Validation("signature", "Signature invalide.")));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Base", connectionString);
        builder.UseSetting("Authentik:Issuer", "https://auth.test.invalid/application/o/badpig/");
        builder.UseSetting("Authentik:Audience", "badpig-tests");
        builder.UseSetting("Announcements:StoragePath", _storagePath);
        builder.UseSetting("Announcements:SiteUrl", "https://test.badpig.fr");
        builder.UseSetting("Shop:SiteUrl", "https://test.badpig.fr");
        builder.UseSetting("Authentik:ApiBase", "https://auth.test.invalid/api/v3");
        builder.UseSetting("Mailbox:WebmailUrl", "https://webmail.test.invalid");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IIdentityProvider>();
            services.AddSingleton(Identity);
            services.RemoveAll<IEmailSender>();
            services.AddSingleton(Mail);
            services.RemoveAll<IDateTimeProvider>();
            services.AddSingleton(Clock);
            services.RemoveAll<IPaymentProvider>();
            services.AddSingleton(Payments);
            services.RemoveAll<IMailboxReader>();
            services.AddSingleton(Mailbox);

            services.AddAuthentication(o =>
                {
                    o.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    o.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    o.DefaultForbidScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && Directory.Exists(_storagePath))
            Directory.Delete(_storagePath, recursive: true);
    }
}
