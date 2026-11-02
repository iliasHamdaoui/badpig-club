using BadPig.Administration;
using BadPig.Announcements;
using BadPig.Interclubs;
using BadPig.Shared.Identity;
using BadPig.Shared.Mailbox;
using BadPig.Shared.Payments;
using BadPig.Orders;
using BadPig.Shop;
using BadPig.Shared.Core;
using BadPig.Shared.Infrastructure;
using BadPig.Shared.Persistence;
using Npgsql;

// Modular monolith in a single project: each folder (Administration, Profile) is a module that registers
// its own services; this file configures authentication and authorization, assembles the modules and starts the app.
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// --- Authentication: tokens issued by the identity provider (the only place that knows which one) ---
builder.Services.AddIdentityProvider(configuration);
builder.Services.AddIdentityAuthentication(configuration);

// --- Online payments: the provider (the only place that knows which one) ---
builder.Services.AddPaymentProvider(configuration);

// --- Club mailbox: only counts the unread messages, so the board knows new mail waits ---
builder.Services.AddClubMailbox(configuration);

// --- Authorization: groups come from the token ("groups" claim) ---
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.Member, p => p.RequireAuthenticatedUser().RequireAssertion(c =>
        c.User.FindAll("groups").Any(g => Groups.IsClubGroup(g.Value))))
    .AddPolicy(Policies.Board, p => p.RequireAuthenticatedUser().RequireAssertion(c =>
        c.User.FindAll("groups").Any(g => g.Value is Groups.Board or Groups.Admin)))
    .AddPolicy(Policies.Admin, p => p.RequireAuthenticatedUser().RequireAssertion(c =>
        c.User.FindAll("groups").Any(g => g.Value == Groups.Admin)));

// Controllers of all the modules (Features folders) are discovered in this assembly.
// JSON answers and bodies are camelCase by default (no [JsonPropertyName] needed on the request/response models).
builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase);
builder.Services.AddProblemDetails();

// --- OpenAPI: the document describes every endpoint (see the [Tags]/[EndpointSummary] attributes of the Endpoint classes) ---
builder.Services.AddOpenApi(o => o.AddDocumentTransformer((document, _, _) =>
{
    document.Info.Title = "BadPig API";
    document.Info.Version = "v1";
    document.Info.Description = "API of the Badminton Pignan club site. Reached only through the site (BFF); sign-in is done by the identity provider.\n\n" +
        "**Authentication**: a valid session (Bearer token added by the site). 401 = not signed in; 403 = signed in but not allowed (board/admin rules). Exception: reading the announcements (`GET /announcements...`) is open to visitors who are not signed in, who only see the public ones.\n\n" +
        "**Errors**: problem details (RFC 9457); `title` is a message in French meant to be shown to the person; 400 also lists the invalid fields in `errors`. 404 = unknown identifier; 409 = conflicts with the current state; 500 = missing server configuration; 502 = identity provider unreachable.\n\n" +
        "**Lists**: query parameters `search`, `sort`, `direction` (Asc/Desc), `page` (from 1) and `pageSize` (25 by default, 100 at most); the answer holds `items`, `page`, `pageSize`, `totalCount`, `totalPages`.\n\n" +
        "**Format**: JSON in camelCase; dates in ISO 8601.";
    // Relative server: "Try it out" calls the same address the page was opened from (the BFF, which adds the token).
    document.Servers = [new Microsoft.OpenApi.OpenApiServer { Url = "/" }];
    return Task.CompletedTask;
}));

// --- Modules ---
builder.Services.AddSharedServices(configuration);
builder.Services.AddAdministrationModule(configuration);
builder.Services.AddAnnouncementsModule(configuration);
builder.Services.AddShopModule(configuration);
builder.Services.AddOrdersModule(configuration);
builder.Services.AddInterclubsModule();

var app = builder.Build();

// Creates or updates the database tables before receiving requests.
var connectionString = configuration.GetConnectionString("Base")
    ?? throw new InvalidOperationException("Missing configuration: ConnectionStrings__Base");
await using (var dataSource = NpgsqlDataSource.Create(connectionString))
{
    await SqlMigrations.Apply(dataSource, app.Logger);
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// The OpenAPI document is only for the board (the API is reached through the BFF, with the session of the signed-in person).
app.MapOpenApi("/" + ApiRoutes.V1 + "/openapi/{documentName}.json").RequireAuthorization(Policies.Board);

// Swagger UI: local development only. Open it through the site (http://localhost:8080/api/v1/swagger) after signing in:
// the BFF adds the session's token to every request.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(o =>
    {
        o.RoutePrefix = ApiRoutes.V1 + "/swagger";
        o.DocumentTitle = "BadPig API";
        o.SwaggerEndpoint("/" + ApiRoutes.V1 + "/openapi/v1.json", "BadPig API v1");
        // The BFF requires this header on calls that modify data (anti-CSRF).
        o.UseRequestInterceptor("(req) => { req.headers['X-BadPig-CSRF'] = '1'; return req; }");
    });
}

app.Run();

// Lets the integration tests (api/tests) start the application in memory.
public partial class Program;
