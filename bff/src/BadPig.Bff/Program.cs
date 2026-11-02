using System.Net.Http.Headers;
using System.Security.Claims;
using BadPig.Bff;
using Microsoft.AspNetCore.Authentication;
using Yarp.ReverseProxy.Transforms;

// BFF (Backend For Frontend): serves the Angular site, handles the OIDC sign-in (confidential client), keeps the tokens
// on the server (the browser only has an HttpOnly cookie) and relays /api calls to the API, adding the token.
var builder = WebApplication.CreateBuilder(args);

var oidcConfig = builder.Configuration.GetSection("Oidc");
var authority = oidcConfig["Authority"] ?? throw new InvalidOperationException("Missing configuration: Oidc__Authority");
var clientId = oidcConfig["ClientId"] ?? throw new InvalidOperationException("Missing configuration: Oidc__ClientId");
var clientSecret = oidcConfig["ClientSecret"];
if (string.IsNullOrWhiteSpace(clientSecret))
    throw new InvalidOperationException("Missing configuration: Oidc__ClientSecret");

builder.Services.AddSingleton<Sessions>();
builder.Services.AddHttpClient("authentik", http => http.Timeout = TimeSpan.FromSeconds(10));
// Link previews read the public announcements from the API (same address as the proxy), without any token.
builder.Services.AddHttpClient(LinkPreview.HttpClientName, http =>
{
    http.Timeout = TimeSpan.FromSeconds(3);
    var apiAddress = builder.Configuration["ReverseProxy:Clusters:api:Destinations:api:Address"];
    if (apiAddress is not null)
        http.BaseAddress = new Uri(apiAddress);
});

builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultScheme = "Cookies";
        o.DefaultChallengeScheme = "Cookies"; // without a session: 401 (no redirect for the site's calls)
    })
    .AddCookie("Cookies", o =>
    {
        o.Cookie.Name = "__Host-badpig";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.Path = "/";
        o.ExpireTimeSpan = TimeSpan.FromHours(12);
        o.SlidingExpiration = true;
        o.Events.OnRedirectToLogin = c => { c.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
        o.Events.OnRedirectToAccessDenied = c => { c.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
        // Valid cookie but lost session (BFF restart, purge, refresh refused): the cookie is rejected.
        o.Events.OnValidatePrincipal = async c =>
        {
            var sid = c.Principal?.FindFirstValue("sid");
            if (sid is null || !c.HttpContext.RequestServices.GetRequiredService<Sessions>().Exists(sid))
            {
                c.RejectPrincipal();
                await c.HttpContext.SignOutAsync("Cookies");
            }
        };
    })
    .AddOpenIdConnect("oidc", o =>
    {
        o.SignInScheme = "Cookies";
        o.Authority = authority;
        o.ClientId = clientId;
        o.ClientSecret = clientSecret;
        o.ResponseType = "code";
        o.UsePkce = true;
        o.SaveTokens = true;                       // during sign-in only (see OnTicketReceived)
        o.GetClaimsFromUserInfoEndpoint = false;
        o.MapInboundClaims = false;
        o.CallbackPath = "/signin-oidc";
        o.SignedOutCallbackPath = "/signout-callback-oidc";
        o.SignedOutRedirectUri = "/";
        o.Scope.Clear();
        o.Scope.Add("openid");
        o.Scope.Add("profile");
        o.Scope.Add("email");
        o.Scope.Add("offline_access");             // refresh token
        o.TokenValidationParameters.NameClaimType = "name";

        // Successful sign-in: the tokens go into the server's memory; the cookie only keeps "sid" and "sub".
        o.Events.OnTicketReceived = c =>
        {
            var accessToken = c.Properties?.GetTokenValue("access_token");
            var sub = c.Principal?.FindFirstValue("sub");
            if (accessToken is null || sub is null)
            {
                c.Fail("Incomplete response from Authentik.");
                return Task.CompletedTask;
            }

            var sid = c.HttpContext.RequestServices.GetRequiredService<Sessions>()
                .Create(accessToken, c.Properties!.GetTokenValue("refresh_token"), c.Properties!.GetTokenValue("id_token"));
            c.Principal = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim("sid", sid), new Claim("sub", sub)], "Cookies"));
            c.Properties!.StoreTokens([]);
            return Task.CompletedTask;
        };

        // Sign-in callback refused (code already used after a double request, timeout...): no raw error page,
        // we go back to the home page; if a first request succeeded, the session already exists.
        o.Events.OnRemoteFailure = c =>
        {
            c.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("SignIn")
                .LogWarning("Sign-in callback refused: {Message}", c.Failure?.Message);
            c.Response.Redirect("/");
            c.HandleResponse();
            return Task.CompletedTask;
        };

        // Sign-out: Authentik receives the session's ID token as a hint, then the session is destroyed.
        o.Events.OnRedirectToIdentityProviderForSignOut = c =>
        {
            var sessions = c.HttpContext.RequestServices.GetRequiredService<Sessions>();
            var sid = c.HttpContext.User.FindFirstValue("sid");
            if (sid is not null)
            {
                c.ProtocolMessage.IdTokenHint = sessions.IdToken(sid);
                sessions.Remove(sid);
            }

            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("authenticated", p => p.RequireAuthenticatedUser());

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transforms => transforms.AddRequestTransform(t =>
    {
        // The token was prepared by the /api middleware below; the browser cookie is never passed on to the API.
        t.ProxyRequest.Headers.Remove("Cookie");
        // Visitors without a session can read the announcements: the API then gets no token (and never a token sent by the browser).
        t.ProxyRequest.Headers.Authorization = t.HttpContext.Items["token"] is string token ? new AuthenticationHeaderValue("Bearer", token) : null;
        return ValueTask.CompletedTask;
    }));

var app = builder.Build();

app.UseExceptionHandler(a => a.Run(c =>
{
    c.Response.StatusCode = StatusCodes.Status500InternalServerError;
    return Task.CompletedTask;
}));

// Security headers.
app.Use(async (c, next) =>
{
    var h = c.Response.Headers;
    if (c.Request.IsHttps)
    {
        h["Strict-Transport-Security"] = "max-age=31536000"; // the browser will only use https for this site
    }

    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "DENY";
    h["Referrer-Policy"] = "strict-origin-when-cross-origin";
    // Local development only: Swagger UI of the API needs inline scripts and eval (request interceptor).
    var swagger = app.Environment.IsDevelopment() && c.Request.Path.StartsWithSegments("/api/v1/swagger");
    h["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self'" + (swagger ? " 'unsafe-inline' 'unsafe-eval'" : "") + "; style-src 'self' 'unsafe-inline'; img-src 'self' data:; " +
        "connect-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self' " + new Uri(authority).GetLeftPart(UriPartial.Authority);
    await next();
});

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = c =>
    {
        // Compiled files (hashed name): long cache; everything else is revalidated every time.
        var fileName = c.File.Name;
        var immutable = System.Text.RegularExpressions.Regex.IsMatch(fileName, @"^(chunk|main|polyfills|styles)-.+\.(js|css)$");
        c.Context.Response.Headers.CacheControl = immutable ? "public, max-age=2592000, immutable" : "no-cache";
    },
});

// --- Link previews: a messaging app reading the address of a public announcement gets the title, excerpt and poster ---
app.Use(async (c, next) =>
{
    if (LinkPreview.AnnouncementIdFor(c.Request) is { } announcementId && app.Environment.WebRootFileProvider.GetFileInfo("index.html") is { Exists: true } index)
    {
        // Behind the reverse proxy the public address is in the forwarded headers.
        var scheme = c.Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? c.Request.Scheme;
        var host = c.Request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? c.Request.Host.Value;
        using var reader = new StreamReader(index.CreateReadStream());
        var page = await LinkPreview.RenderAsync(
            c.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient(LinkPreview.HttpClientName),
            await reader.ReadToEndAsync(c.RequestAborted), $"{scheme}://{host}", announcementId, c.RequestAborted);
        if (page is not null)
        {
            c.Response.ContentType = "text/html; charset=utf-8";
            c.Response.Headers.CacheControl = "no-cache";
            await c.Response.WriteAsync(page, c.RequestAborted);
            return;
        }
    }

    await next();
});

app.UseAuthentication();
app.UseAuthorization();

// --- Calls to the API: session required, anti-CSRF header required, a valid token is added ---
app.Use(async (c, next) =>
{
    if (!c.Request.Path.StartsWithSegments("/api"))
    {
        await next();
        return;
    }

    // The proxy route itself is open ("AuthorizationPolicy": "anonymous" in appsettings.json): the session is checked here, because a few calls
    // have no session (reading the public announcements, reading the hours of the club, notifications of the payment provider).
    // No session: 401 (the site then sends the person to the sign-in page), except for the reads open to visitors:
    // the announcements (the API decides what the reader may see) and when the club plays.
    var sid = c.User.FindFirstValue("sid");
    if (sid is null && IsOpenToVisitors(c.Request))
    {
        await next();
        return;
    }

    // The payment provider calls the API directly (no session, no anti-CSRF header): the API trusts the notification only if its signature is right.
    if (IsPaymentNotification(c.Request))
    {
        await next();
        return;
    }

    if (sid is null)
    {
        c.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }

    // A custom header cannot be sent by another origin without permission: protects against CSRF.
    if (!HttpMethods.IsGet(c.Request.Method) && !HttpMethods.IsHead(c.Request.Method) && !c.Request.Headers.ContainsKey("X-BadPig-CSRF"))
    {
        c.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    var token = await c.RequestServices.GetRequiredService<Sessions>().ValidAccessToken(sid, c.RequestAborted);
    if (token is null)
    {
        c.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return;
    }

    c.Items["token"] = token;
    await next();
});

// --- Sign-in / sign-out / identity ---
app.MapGet("/bff/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.MapGet("/bff/login", (string? returnUrl) =>
{
    // Only addresses inside the site are accepted (no redirect to another site).
    var target = returnUrl is { Length: > 0 } && returnUrl[0] == '/' && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\")
        ? returnUrl
        : "/";
    return Results.Challenge(new AuthenticationProperties { RedirectUri = target }, ["oidc"]);
}).AllowAnonymous();

// Sign-out: POST request from the site's form (another site cannot send it with the "SameSite=Lax" cookie).
app.MapPost("/bff/logout", () => Results.SignOut(new AuthenticationProperties { RedirectUri = "/" }, ["Cookies", "oidc"])).AllowAnonymous();

app.MapGet("/bff/user", async (HttpContext c) =>
{
    var sid = c.User.FindFirstValue("sid");
    var token = sid is null ? null : await c.RequestServices.GetRequiredService<Sessions>().ValidAccessToken(sid, c.RequestAborted);
    var payload = token is null ? null : AccessToken.Parse(token);
    if (payload is null)
        return Results.Unauthorized();

    c.Response.Headers.CacheControl = "no-store";
    return Results.Ok(new { sub = payload.Sub, name = payload.Name, groups = payload.Groups });
}).AllowAnonymous();

app.MapReverseProxy();

// Angular application: any other address returns index.html.
app.MapFallbackToFile("index.html", new StaticFileOptions
{
    OnPrepareResponse = c => c.Context.Response.Headers.CacheControl = "no-cache",
});

app.Run();

// Reads open to a visitor without a session: the announcements (the API only returns the public ones to them)
// and the hours of the club, which the home page shows to everybody.
static bool IsOpenToVisitors(HttpRequest request) =>
    HttpMethods.IsGet(request.Method)
    && (request.Path.StartsWithSegments("/api/v1/announcements") || request.Path.Equals("/api/v1/club/sessions"));

// Notification sent by the payment provider (signed; checked by the API).
static bool IsPaymentNotification(HttpRequest request) =>
    HttpMethods.IsPost(request.Method) && request.Path.Equals("/api/v1/shop/payments/notifications");
