using System.Security.Claims;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BadPig.Api.Tests.Infrastructure;

/// <summary>
/// Replaces the check of the identity provider's token: the claims of the token ("sub", "name", "groups") are read from
/// request headers, which is what the BFF/IDP chain provides in production. No "sub" header = nobody signed in (401).
/// </summary>
public sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string AccountHeader = "X-Test-Account";
    public const string NameHeader = "X-Test-Name";
    public const string GroupsHeader = "X-Test-Groups";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(AccountHeader, out var account))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim> { new("sub", account.ToString()), new("name", Request.Headers[NameHeader].ToString()) };
        var groups = Request.Headers[GroupsHeader].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
        claims.AddRange(groups.Select(g => new Claim("groups", g)));

        var identity = new ClaimsIdentity(claims, SchemeName, nameType: "name", roleType: ClaimTypes.Role);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }
}
