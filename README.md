# BadPig – badminton club website

One repository, two applications:

| Folder | Contents |
|---|---|
| `web/` | Angular website (public pages, announcements, members area, profile, administration) |
| `bff/` | ASP.NET Core BFF: serves the Angular website, handles the OIDC sign-in (HttpOnly cookie, tokens kept server-side) and forwards `/api` to the API (YARP) |
| `api/` | ASP.NET Core API (`/api/v1/...`), modular monolith (see below): Authentik token validation, accounts, memberships, board, audit log |
| `deploy/test/` | Deployment files for the test environment (Docker Compose, Traefik) |
| `deploy/prod/` | Reserved for production (step 27) |

Docker images are built by GitHub Actions (`.github/workflows/build-test.yml`): `api` with the `api/` context, `web` (the BFF and the website) with the repository root as context and `bff/Dockerfile`.

## API structure (modular monolith, vertical slices)

```
api/
  BadPig.sln
  src/BadPig.Api/       a single .NET project; one folder per module
    Program.cs          authentication, policies, module wiring, SQL scripts at startup
    Migrations/         SQL schema scripts (applied at startup)
    Health/             monitoring endpoint
    Shared/             common core: groups, policies, account rules, e-mails, AppDbContext (Shared/Persistence, common part)
      Identity/         everything that talks to the IDP (Authentik); modules only use IIdentityProvider
      Payments/         everything that talks to the payment provider (Stripe); modules only use IPaymentProvider
    Administration/     administrative operations of the board: invitations, accounts, memberships, board, titles, audit log
    Profile/            what a person does for themselves: my profile, delete my account
    Shop/               shop management by the board: products, variants, stock and its history, incoming orders (`/admin/shop/...`)
    Orders/             member orders: catalogue, ordering, online payment, my orders (`/shop/...`)
    Interclubs/         interclub competition: teams and captains (board), fixture calendar (captain or board), read by members (`/interclubs/...`, `/admin/interclubs/...`)
    Announcements/      club announcements: written by the board, read by visitors and members, likes, comments, polls, poster and attachments
      Core/             shared between use cases, grouped by subject
      Features/         one folder per use case: Endpoint (HTTP) + Handler (logic) + Request/Response
      Infrastructure/   EF Core (the module's part of AppDbContext), e-mails, background jobs
```

- A module only uses another module's `Core` folder (interfaces such as `IMembershipReader`, `IBoardInfo`, `IAdministrationAudit`) and `Shared` (including `IIdentityProvider`), never its `Features` nor its `Infrastructure`. This is a written rule (single project: the compiler does not enforce it).
- A single `AppDbContext` (EF Core, one PostgreSQL database), as partial classes: each module owns `Infrastructure/AppDbContext.<Module>.cs` with its `DbSet` properties and the configuration of its entities. The schema is still created by the SQL scripts in `Migrations/` (001 to 032); EF migrations will replace them.
- Use cases are grouped by actor: `Administration` = administrative operations of the board, `Profile` = what a person does on their own account. A module that needs an audit log defines its own; today `IAdministrationAudit` (table `administration_audit_log`) records every board operation, memberships included. JSON is camelCase by default.
- A feature never depends on the `Infrastructure` folder (only on `Core`, `Shared` and its own types); one action = one feature.
- Cohesion: anything used by a single use case (models, audit constants, e-mails, enumerations) lives in its folder; `Core`/`Infrastructure` only hold what is shared between several use cases or called by another module.
- Clock: `IDateTimeProvider` (never `DateTime.UtcNow`). Lists: `?search=&sort=&direction=&page=&pageSize=` and the response `{ items, page, pageSize, totalCount, totalPages }` (`admin/users`, `admin/titles`, `admin/audit-log`). `pageSize` is 25 by default and 100 at most; the site lets the person pick 10, 25, 50 or 100.
- Every use case lives in `Features/<UseCaseName>/` (its own namespace): `…Endpoint` (thin HTTP controller), `…Handler` (handles the request), `…Request` / `…Response` (models). The OpenAPI document (native library) is served on `/api/v1/openapi/v1.json` for the board. Routes and JSON fields are the contract with the website (the `*.api.ts` services of the website modules, in `web/src/app/modules/*/infrastructure/`).
- Account deletion: each module erases its own data (`IAccountErasure`).

Hosting, Authentik, Traefik, PostgreSQL and mail: see `documentation-vps-badpig.md` (server installation documentation).

## Security principles

- Sign-in goes through **Authentik** (OpenID Connect, code + PKCE) and is handled by the **BFF** (confidential client: the secret stays server-side). The browser only holds an `HttpOnly` session cookie (`__Host-badpig`, `SameSite=Lax`); tokens stay in the BFF's memory, which refreshes them on its own. The website never sees a password.
- `/api` calls always go through the BFF (session required, anti-CSRF header `X-BadPig-CSRF` for requests that modify data, token added by the BFF). **The API is no longer exposed by Traefik**: only the BFF reaches it, over the Docker network. When the BFF restarts, sessions are lost: everyone signs in again.
- The Angular guards only drive what is displayed. **The real protection is in the API**: on every call it checks the token signature, the issuer, the audience and the groups (`Membres`, `Bureau`, `Admin`).
- A member is identified by their **Authentik UUID** (`sub`), never by their e-mail or their name.
- The API talks to Authentik with a **service account** (`badpig-api-test`) whose token is never in Git: it comes from the server's `.env` file.
- Test: the website sits behind Traefik + Authentik (groups `Bureau`/`Admin`); `/api` does not (protected by the BFF cookie, then by token validation in the API).
- BFF endpoints: `/bff/login?returnUrl=…` (sign in), `POST /bff/logout`, `GET /bff/user` (name and groups, 401 without a session), `/bff/health`. OIDC URLs to declare in Authentik: `https://<site>/signin-oidc` and `https://<site>/signout-callback-oidc`.

## API tests

Project `api/tests/BadPig.Api.Tests`: xUnit v3, Testcontainers (PostgreSQL 17) and Respawn. The tests call the public API (HTTP) on the application started in memory; only Authentik, e-mails, the clock and token validation are replaced. Docker must be running.

```powershell
cd api
dotnet test BadPig.sln
# Faster locally: keep the PostgreSQL container between runs
$env:BADPIG_TEST_DB_REUSE = "true"
```

To connect to the test database (DBeaver, psql…), set the port and the password before running the tests (useful with `BADPIG_TEST_DB_REUSE`, otherwise the database disappears at the end):

```powershell
$env:BADPIG_TEST_DB_REUSE = "true"
$env:BADPIG_TEST_DB_PORT = "55432"
$env:BADPIG_TEST_DB_PASSWORD = "<pick a local password>"
```

Connection: host `localhost`, port `55432`, user `postgres`, database `postgres`. The password is never written in the repository.

From Visual Studio or Rider: select `api/tests/BadPig.Api.Tests/.runsettings` (it sets the same variable). From the command line: `dotnet test --settings tests/BadPig.Api.Tests/.runsettings`.

`global.json` (at the root) enables the `Microsoft.Testing.Platform` runner for `dotnet test`, required with xUnit v3 on the .NET 10 SDK.

Tests are named `Should_ExpectedBehaviorOrAction_When_Condition`. See `CLAUDE.md` for the rules.

## API endpoints (`/api/v1`)

| Method | Address | Access |
|---|---|---|
| GET | `/health` | public (monitoring) |
| GET | `/me` | signed in: the profile, with the **licence number** (`licence`, `null` if none) and the **membership date** (`memberSince` = first validated membership, `null` if none) and the **picture version** (`photoVersion`, `null` if none) |
| PUT | `/me` | group `Membres`, `Bureau` or `Admin`: changes one's own name and e-mail (in Authentik); 204, read the profile again with `GET /me` |
| PUT | `/me/licence` | group `Membres`, `Bureau` or `Admin`: saves **one's own** licence number (body `{ number }`, letters, digits and hyphens, 20 characters at most; empty = removes the licence); 204. 409 when another person already has that number (upper and lower case are the same number) |
| PUT | `/me/photo` | group `Membres`, `Bureau` or `Admin`: sets **one's own** profile picture (`multipart/form-data`, field `file`: JPEG, PNG or WebP, 1 MB at most; the type is checked on the first bytes of the file); it replaces the previous one; 204. The website sends a square JPEG that the person centred and zoomed in the browser. The picture is kept in the table `member_photos` and erased with the account |
| DELETE | `/me/photo` | the same people: removes one's own picture; 204 (also when there is none) |
| GET | `/members/{id}/photo` | **members with an up-to-date membership and board**: the picture of a person (403 otherwise, 404 when they have none). The address carries the picture version (`?v=`, see `photoVersion` in `GET /me`, the user list, the comments and the directory), so the browser keeps it |
| GET | `/club/directory` | same readers: the **club directory (trombinoscope)** — active members **who put a picture** (a member without a picture is not listed) with `id`, `name`, `photoVersion` (`?search=<name>&sort=name`, paginated) |
| DELETE | `/me` | group `Membres`, `Bureau` or `Admin`: permanently deletes one's own account (refused for the last `Admin`) |
| POST | `/admin/invitations` | `Bureau` or `Admin`: invites a person (body `{ name, email, group, membershipKind }`, `membershipKind` = `internal` (licensed with the club, the default) or `external` (licensed with another club): it decides which fee applies on validation; `group` = `members` by default, `board` (the person is in `Membres` and `Bureau`) or `admin` (reserved to `Admin`; `Membres`, `Bureau` and `Admin`, group UUID in `Authentik__AdminGroup`); creates the account and sends a 3-day link to choose a password; status "waiting for signup"); response 201 `{ id }` (identifier of the created account) |
| POST | `/admin/users/{id}/resend-invitation` | `Bureau` or `Admin`: sends the link again to a person who has never signed in (even if their account was created directly in Authentik); 409 if they have already signed in |
| PUT | `/admin/users/{id}/licence` | `Bureau` or `Admin`: saves a person's licence number (body `{ number }`, empty = removes it; 404 if the account does not exist, 409 when another person already has that number); recorded in the audit log (`licence.updated`) |
| POST | `/admin/users/{id}/board/add` · `/board/remove` | `Bureau` or `Admin`: adds to / removes from the `Bureau` group (the person stays in `Membres`). Never on oneself, there is always at least one board member, `Admin` accounts can only be changed by an `Admin`; recorded in the audit log |
| PUT | `/admin/users/{id}/title` | `Bureau` or `Admin`: grants a title (`titleId`) or removes it (`null`); only for an active person in the `Bureau` group; a title has a single holder (it moves to the new person); lost when leaving the board; recorded in the audit log |
| GET | `/club/board` | **members and board** (403 for a visitor): board composition — name, e-mail and title of every active member of the `Bureau` group (`?search=<name or title>&sort=name\|title`, title holders first when sorting by title). Board e-mails are never visible to a visitor |
| GET · POST · PUT · DELETE | `/admin/titles`, `/admin/titles/{id}` | `Bureau` or `Admin`: lists, creates, renames, deletes board titles (unique name, 60 characters max; deleting a title removes it from its holder); recorded in the audit log; creation answers 201 `{ id }` |
| POST | `/admin/users/{id}/membership/validate` (body `{ kind? }`) · `/cancel` | `Bureau` or `Admin`: validates / cancels the membership for the current season. Validation stores the kind (`internal` or `external`, by default the one chosen at invitation time) and the season fee as it stands at that moment. Never on oneself; 409 if no season is open |
| GET · POST · PUT · DELETE | `/admin/seasons`, `/admin/seasons/{id}` | `Bureau` or `Admin`: lists (search, sort `startsOn`/`name`), creates (201 `{ id }`), updates, deletes seasons: unique name, non-overlapping dates (defaults: end = start + 1 year − 1 day, name `2026-2027`), grace period in days (30 by default), fees `internalFee` / `externalFee`. Deletion refused (409) when validated memberships **or interclub teams** exist |
| GET · POST · PUT · DELETE | `/admin/seasons/{id}/holidays`, `/admin/seasons/{id}/holidays/{holidayId}` | `Bureau` or `Admin`: the **school holidays** of a season (`{ name, startsOn, endsOn }`, both days included): lists (search on the name, sort `startsOn` by default or `name`), adds (201 `{ id }`), updates, deletes. 400 if the dates fall outside the season, 404 if the season (or the period) does not exist, 409 if the dates overlap another period. Every action is recorded in the audit log (`holiday.created\|updated\|deleted.<name>`) |
| GET | `/club/sessions` | **open to everyone, without signing in**: when the club plays. Returns today's date in Paris, the weekly schedule (`weekly`), `isOpenToday`, the holiday currently running (`closure` = `{ name, endsOn }`, `null` when the club is open) and the next sessions (`next`, `?count=` from 1 to 10, 3 by default; the first one may be today's, with `isToday`). The seasons' school holidays are taken out of the calculation |
| GET | `/admin/seasons/statistics` | `Bureau` or `Admin`: per season, the number of memberships and the income (club-licensed / external), amounts as recorded at validation time |
| POST · PUT · DELETE | `/admin/announcements`, `/admin/announcements/{id}` | `Bureau` or `Admin`: creates (201 `{ id }`, body `{ title, bodyHtml, kind, expiresOn?, commentsEnabled? }`, draft by default), updates, deletes (any board member, files and likes included) an announcement. `kind` = `internal` (members) or `public` (any visitor). `bodyHtml` is sanitised server-side (allow list: headings, bold, italic, lists, quotes, https/http/mailto/tel links…), the summary is a plain-text excerpt. Videos and links (YouTube, Vimeo, Instagram, Facebook…) are written in the text: the website turns a YouTube or Vimeo link alone on its line into a player (loaded on click), other links stay links. `expiresOn` (`yyyy-MM-dd`, Paris date, not in the past): last day of display, **90 days after creation by default**; on update, `null` = unchanged. `commentsEnabled` (default `true`): allows comments; on update, `null` = unchanged |
| POST | `/admin/announcements/{id}/publish` · `/unpublish` · `/pin` · `/unpin` | `Bureau` or `Admin`: publishes (the first publication date is kept) / unpublishes; pins at the top of the list / unpins |
| PUT · DELETE | `/admin/announcements/{id}/poster` | `Bureau` or `Admin`: replaces (multipart, field `file`, JPEG/PNG/WebP/GIF image of 5 MB at most, signature checked) / removes the poster |
| POST · DELETE | `/admin/announcements/{id}/attachments`, `/attachments/{fileId}` | `Bureau` or `Admin`: adds (201 `{ id }`, multipart `file`, 10 MB at most, 10 files at most → 409, PDF, images, Office, OpenDocument, text; type decided by the extension) / removes an attachment |
| POST | `/admin/announcements/polls` | `Bureau` or `Admin`: creates a **poll**, which is a full announcement (`form` = `poll`, 201 `{ id }`, draft). Body `{ title (the question), bodyHtml? (optional explanation), kind, options: [2 to 10 different texts], allowMultiple, isAnonymous, closesOn, expiresOn?, commentsEnabled? }`. It is published, pinned, updated (question, explanation, kind, end of display: `PUT /admin/announcements/{id}`) and deleted like an announcement; the options never change. It stays displayed at least until it closes |
| PUT | `/admin/announcements/{id}/poll/deadline` | `Bureau` or `Admin`: changes the deadline (body `{ closesOn }`) of a poll that is still open, even with answers (409 once closed) |
| PUT · DELETE | `/announcements/{id}/poll/vote` | members and board (403 for a visitor): answers a published poll (body `{ optionIds }`, a single option when `allowMultiple` is false), replacing the previous answer / withdraws one's answer. Possible until the deadline, that day included; afterwards 409 |
| GET | `/announcements`, `/announcements/{id}` | **public (no sign-in)**: paginated list (`?kind=&form=text\|poll&status=&search=&sort=publishedAt\|createdAt\|title&direction=`) sorted pinned first, then most recent. Visitor (not signed in, or a member whose membership is not validated): published public announcements; member: published internal and public ones; board: everything, drafts included, with the author (invisible to the others). Like count and `likedByMe`: members and board only. Each item has a plain-text `summary` and a formatted `previewHtml` (the first few blocks of the text, cut between two blocks, titles/lists/emphasis kept, links turned into text) with `hasMore` when the text goes on (the website shows « Lire la suite ») |
| GET · POST | `/announcements/{id}/comments` | members and board (403 for a visitor), on a published and readable announcement or poll (404 otherwise): paginated list of the conversation (`?search=&sort=createdAt&direction=`, oldest first), each message with `isMine` / `canDelete` and, when it answers another one, `replyTo` (`{ id, authorName, excerpt (120 characters at most), isDeleted }`); adds a comment (body `{ text, parentId? }`, plain text from 1 to 1000 characters, 201 `{ id }`). `parentId` = the message being answered (any message of the announcement, replies included; 404 if it does not exist on that announcement). 409 when comments are closed |
| DELETE | `/announcements/{id}/comments/{commentId}` | members and board: a member deletes **their own** comment (403 otherwise), any board member deletes any comment. Messages that quoted it stay, with a "Message supprimé" quote. 404 if the comment is not on that announcement. Still possible when comments are closed |
| GET | `/announcements/{id}/files/{fileId}` | same visibility as the announcement: poster (displayed in the page) or attachment (downloaded) |
| POST · DELETE | `/announcements/{id}/like` | group `Membres` with a validated membership (or board/admin): likes / removes the like (idempotent, only on a visible published announcement) |
| POST · GET | `/admin/shop/products` | `Bureau` or `Admin`: creates a product (201 `{ id }`, body `{ name, description? }`) / paginated list of products **including hidden ones**, with their variants and their stock (`?search=&lowStock=true&sort=name\|createdAt`); a variant exposes `quantity` (in stock), `reserved` (held by unpaid orders), `available`, `lowStockThreshold`, `isLowStock` |
| PUT · DELETE | `/admin/shop/products/{id}` | `Bureau` or `Admin`: updates (`{ name, description, isActive }`) / deletes a product and its variants (409 as long as unpaid orders hold stock: hide it instead) |
| POST | `/admin/shop/products/{id}/variants` | `Bureau` or `Admin`: adds a variant (201 `{ id }`, body `{ label, priceCents, quantity?, lowStockThreshold? }`, label unique within the product → 409): a shuttlecock type, a shirt size + men/women… each with its price in cents and its stock |
| PUT · DELETE | `/admin/shop/variants/{id}` | `Bureau` or `Admin`: updates (`{ label, priceCents, lowStockThreshold, isActive }`, orders already placed keep their price) / deletes a variant (409 if stock is held) |
| POST | `/admin/shop/variants/{id}/restock` | `Bureau` or `Admin`: adds received units (`{ quantity, note? }`), recorded in the history |
| POST | `/admin/shop/variants/{id}/stock-correction` | `Bureau` or `Admin`: sets the real stock after counting (`{ quantity, note }`, reason required; 409 below the held units); the difference is recorded |
| GET | `/admin/shop/variants/{id}/movements` | `Bureau` or `Admin`: stock history of a variant (`restock`, `correction`, `sale`), most recent first |
| GET | `/admin/shop/orders` | `Bureau` or `Admin`: every order with its lines (`?status=pending\|paid\|delivered\|cancelled\|expired&search=<name>&sort=createdAt\|totalCents\|buyerName`) |
| POST | `/admin/shop/orders/{id}/deliver` | `Bureau` or `Admin`: marks a **paid** order as handed over in person (409 otherwise) |
| GET | `/shop/products` | members in good standing (and board), 403 otherwise: catalogue of the products on offer, with their variants, prices and available stock (`?search=&sort=name`) |
| POST · GET | `/shop/orders` | members in good standing (POST): places an order (body `{ lines: [{ variantId, quantity }] }`, 1 to 20 lines, 1 to 10 units per line) → the stock is **held for 35 minutes**, 201 `{ id }`; 409 if the stock is insufficient, the product was withdrawn or an unpaid order already exists; 502 if Stripe cannot be reached. GET: my orders (`?status=&sort=createdAt\|totalCents`) |
| GET | `/shop/orders/{id}` | my order with its lines and, while it awaits payment, `checkoutUrl` (the Stripe payment page to send the person to). Reading a pending order queries Stripe: when the person comes back from the payment page it is up to date even if the notification is late |
| POST | `/shop/orders/{id}/cancel` | cancels my **unpaid** order (the stock is released, the payment page closed; 409 otherwise) |
| PUT · DELETE | `/admin/shop/products/{id}/image` | `Bureau` or `Admin`: adds or replaces (multipart, field `file`, JPEG/PNG/WebP of 3 MB at most, type checked on the content) / removes a product image (one per product, kept in the database); recorded in the shop audit log |
| GET | `/shop/products/{id}/image` | members in good standing and board: the image of a product on offer (the board also sees those of hidden products); 404 without an image. Product lists give `imageVersion` (null without an image): the address to use is `/shop/products/{id}/image?v={imageVersion}` (the browser may cache it) |
| GET | `/admin/shop/audit-log` | `Bureau` or `Admin`: log of the board's actions on the shop (product/variant created, updated, deleted, restock, stock correction, order handed over), most recent first; `?search=` (author, action, product, details), `?sort=date\|actor\|action\|subject` |
| POST | `/shop/payments/notifications` | **called by Stripe** (webhook, no sign-in): trusted only when the `Stripe-Signature` header is valid (400 otherwise). Payment succeeded → the held stock is sold and the buyer gets an e-mail; page expired → the stock is released. Each order is settled only once |
| POST | `/admin/interclubs/teams` | `Bureau` or `Admin`: creates a team (`{ name, seasonId }`, e.g. `D4` in season 2026-2027) → 201 + identifier; 404 if the season does not exist, 409 if the name already exists in that season |
| PUT · DELETE | `/admin/interclubs/teams/{id}` | `Bureau` or `Admin`: updates the name and the season (`{ name, seasonId }`; 404 if the team or the season does not exist, 409 on a duplicate) / deletes the team and its fixtures |
| PUT · DELETE | `/admin/interclubs/teams/{id}/captain` | `Bureau` or `Admin`: appoints (`{ captainId }`, an active club member, 400 otherwise) / removes the team captain. The captain does not need to be on the board: their rights are limited to their team's fixtures |
| GET | `/interclubs/seasons` | members in good standing (and board): the seasons offered as an interclub filter — those that have teams, plus the **current season** even without teams (`isCurrent`, the one the website selects by default), with `teamCount`; `?search=&sort=name\|start`, most recent first |
| GET | `/interclubs/teams` | members in good standing (and board), 403 otherwise: the teams with their season (`seasonId`, `season`), their captain, `isMyTeam` (I am the captain), `memberCount` and `isPlayer` (I play in the team) (`?seasonId=&search=<team or captain>&sort=name\|season`, most recent season first by default) |
| GET | `/interclubs/fixtures` | members in good standing (and board): the fixture calendar (`?seasonId=&teamId=&when=upcoming\|past\|live&search=<opponent or team>&sort=date\|team\|opponent`); "upcoming" = today (Paris) or later; **`live`** = a fixture **in progress** (today, the 7 matches not all decided, and the Paris time inside a window that opens 15 min before the start and lasts 5 h; a fixture without a start time counts for the whole day). Each row also carries `decidedMatches` (0 to 7), `clubMatches` / `opponentMatches` (matches won so far) and `isLive`, to follow an ongoing entry; `clubScore` / `opponentScore` remain the final score (null before the 7 matches). Sorted by date by default (past ones, most recent first) |
| GET | `/interclubs/fixtures/{id}` | members in good standing (and board): one fixture (date `yyyy-MM-dd`, time `HH:mm:ss` or `null`, opponent, `venue` = `home`\|`away`, place) with the team, the captain, `canManage`, `clubScore`/`opponentScore` (`null` until the result is entered) and `matches`: the 7 matches in the order `SD`, `MX1`, `MX2`, `DD`, `DH`, `SH1`, `SH2` with their players, their `sets` (`{ club, opponent }`) and their `outcome` (`won`/`lost`, **computed** from the sets) |
| POST | `/interclubs/teams/{id}/fixtures` | team captain or board (403 otherwise): adds a fixture (`{ date, startTime?, opponent, venue, place? }`) → 201 + identifier |
| PUT · DELETE | `/interclubs/fixtures/{id}` | team captain or board (403 otherwise): updates / deletes a fixture |
| PUT | `/interclubs/fixtures/{id}/lineup` | team captain or board (403 otherwise): the lineup (`{ slots: [{ slot, playerIds }] }`): 1 player for a single (`SD`, `SH1`, `SH2`), 2 different players for a double or a mixed (`MX1`, `MX2`, `DD`, `DH`), all of them in the team squad (400 otherwise); it replaces the previous lineup, a missing match is emptied (its result stays). No gender check: the club does not know it |
| PUT | `/interclubs/fixtures/{id}/matches/{slot}/result` | team captain or board: **a single match** (`slot` = `SD`, `MX1`, `MX2`, `DD`, `DH`, `SH1`, `SH2`), to enter results **as the evening goes**: body `{ ending, withdrawn, sets }`, same rules as below. The other matches are left untouched; the fixture score stays unknown until the 7 matches are decided. 400 if the match is unknown or the entry invalid, 403, 404, 409 before the day of the fixture |
| PUT | `/interclubs/fixtures/{id}/result` | team captain or board: **the 7 matches at once** (`{ matches: [{ slot, ending, withdrawn, sets: [{ club, opponent }] }] }`). **Rules**: a set is won at **15 points with a 2-point gap** (15-13 or less); from 14-14 on, a **2-point gap** (16-14, 17-15…) or the **first to 21** (21-19, 21-20); a match has **2 or 3 sets** and is won with **2 sets** (no 3rd set after 2-0, 3rd set required at 1-1), 400 otherwise. `ending`: `played` (the default, `withdrawn` not allowed), `forfeit` (match not played: no set) or `retirement` (retired during the match: the finished sets may be given, possibly none, without the match being decided); for a forfeit or a retirement, `withdrawn` (`club` or `opponent`) is required and **that side loses the match** (400 otherwise). Nobody enters the winner: each match result is derived from the sets (the higher score wins the set) or from the side that forfeited or retired, and the fixture score from the number of matches won by each side. 409 if the fixture has not taken place yet (Paris date); sending the result again replaces the previous one |
| PUT · DELETE | `/interclubs/teams/{id}/members/{accountId}` | team captain or board (403 otherwise): adds a player (an active club member licensed with the club, 400 otherwise — a member licensed in another club plays the interclubs for that one; idempotent; **409 when the person already plays in another team of the same season**) / removes them (404 if absent; their answers to still-open date proposals are deleted, those of closed proposals stay). The captain adds themselves to the squad if they play |
| GET | `/interclubs/teams/{id}/members` | members in good standing (and board): the team players (`?search=&sort=name`), the captain is flagged |
| GET | `/interclubs/teams/{id}/candidates` | team captain or board: the active members licensed with the club who do not play in **any** team of that season yet (a player belongs to a single team per season) (`?search=&sort=name`, paginated) |
| POST | `/interclubs/teams/{id}/proposals` | team captain or board (403 otherwise): proposes **2 to 10** different dates for the next fixture (`{ opponent, venue, place?, dates: [{ date, startTime? }] }`) to collect availabilities → 201 + identifier |
| GET | `/interclubs/proposals` | members in good standing (and board): the date proposals with, per date, the number of `yes`/`maybe`/`no` and my own answer (`?teamId=&status=open\|chosen&search=&sort=createdAt\|team`, most recent first) |
| GET | `/interclubs/proposals/{id}` | members in good standing (and board): one proposal; the team captain and the board also see **who answered what** (`answers`) |
| PUT | `/interclubs/proposals/{id}/answers` | **players of the team** the proposal belongs to (403 otherwise): my answer to one or several dates (`{ answers: [{ dateId, answer: yes\|maybe\|no }] }`), replacing the previous one; 409 once a date has been chosen |
| POST | `/interclubs/proposals/{id}/choose` | team captain or board: picks a date (`{ dateId }`) → creates the fixture in the calendar (opponent and place taken from the proposal), closes the proposal → 201 + the fixture identifier; 409 if already chosen |
| DELETE | `/interclubs/proposals/{id}` | team captain or board: cancels the proposal and its answers (a fixture already created stays) |
| GET | `/admin/mailbox` | `Bureau` or `Admin`: tells whether new mail waits in the club mailbox — `isAvailable` (false when no mailbox is configured or the mail server cannot be reached), `unreadCount`, `webmailUrl`, `checkedAt`. Only the unread messages are counted, over IMAP, on a mailbox opened read-only: no message is ever downloaded, marked as read or stored. The answer is kept for a minute |
| GET | `/admin/audit-log` | group `Bureau` or `Admin`: log of the board's actions on accounts (read only) |
| GET | `/admin/users` | group `Bureau` or `Admin`: the people of the club (technical accounts hidden), their **licence number** and what can be done on each of them; the search also covers the licence. `?group=members\|board\|admin` keeps one kind of account — `members` means the plain members, those who are neither on the board nor administrators (everybody is in `Membres`, so filtering on that group would show the whole club); an unknown value answers 400. The status is `disabled` (deactivated account), `pending` (**never signed in**: the person has not chosen a password yet) or `active`; the invitation row only tells who invited them and when |
| POST | `/admin/users/{id}/disable` · `/enable` | `Bureau` or `Admin`: deactivates / reactivates an account. An account deactivated for more than a month is deleted automatically (background job every 6 h, "Système" in the audit log; never for `Bureau`/`Admin`) |
| DELETE | `/admin/users/{id}` | `Bureau` or `Admin`: permanently deletes an account |

Account rules (enforced in the API): administration actions never apply to one's own account; the board acts on plain members (`Membres`); `Bureau` and `Admin` accounts can only be managed by an `Admin`.

## Required Authentik settings (one-off)

1. **Subject mode** of the `BadPig test (site web)` provider: "Based on the User's UUID" (Applications > Providers > the provider > Edit > Advanced protocol settings). This is how the API finds the user from `sub`.
2. **Post logout redirect URIs** of the same provider: `https://test.badpig.fr/` (Strict mode), to come back to the website after signing out.
3. **Service account rights** (role `badpig-api` or equivalent): view and update users, **and delete a user** (global permission `Can delete User`).
4. The return address `https://test.badpig.fr/auth/callback` (already created); the `/auth/silent-refresh` route is not used (refresh token instead).

## E-mails to the people concerned

**Seasons and memberships**: seasons are data managed by the board (dates, grace period, two fees). By default from 1 September to 31 August; the current season is the one containing today's date (Europe/Paris), otherwise the last one that started. The previous season's membership is still accepted (`grace`) during the grace period (30 days by default); after that it must be validated (`valid`, `grace`, `to_validate`). A membership is **internal** (licensed with the club) or **external** (licensed with another club) and keeps the season fee as it stood at validation time (statistics). API inputs are validated with FluentValidation (400 response listing the fields in error).

The API sends an automatic e-mail (from `ne-pas-repondre@badpig.fr`, through the VPS Postfix: Docker network `mail`, `Smtp__Host=172.21.0.1`, port 25) to the person concerned for: deactivation (with the automatic deletion date), reactivation, account deletion (by the board or automatic), membership validation, joining the board, leaving the board, a title granted or removed. No e-mail when acting on one's own account. A send failure never cancels the action: it is written to the API logs. An empty `Smtp__Host` means nothing is sent.

## Local development

```bash
# Website (requires Node.js 22)
cd web
npm ci
npx ng build       # output in dist/web/browser

# API (requires the .NET 10 SDK)
cd api/src/BadPig.Api
dotnet run          # (dotnet build api/BadPig.sln compiles every module)
```

The full local test (BFF + API + PostgreSQL) is described further down, see "Local test" (to be completed).

## Shop: products, stock, orders and payment

Two modules, split by actor: `Shop` (board only: products, variants, stock, history, incoming orders and handover) and `Orders` (member side: catalogue, ordering, payment, my orders). They only talk to each other through interfaces of their `Core`: `Orders` reads the catalogue and moves stock through `IShopCatalog` / `IStockKeeper` (provided by `Shop`), and `Shop` reads orders and marks them handed over through `IOrderBook` (provided by `Orders`). The routes do not change. The board keeps the catalogue: a **product** (tube of shuttlecocks, club shirt) has **variants** (shuttlecock type; size + men/women), each with its own price (in cents, e.g. 2000 = €20) and its own stock. Members in good standing (same rule as the members area) and the board order and pay online.

- **Stock**: `quantity` = units on the shelf, `reserved` = units held by unpaid orders, `available` = the difference (this is what members see). Restocking (`restock`) or correcting after a count (`stock-correction`, reason required) writes a line in the **history** (who, when, how many); a sale does too. A per-variant **alert threshold** (`lowStockThreshold`) drives `isLowStock` / `hasLowStock` (filter `?lowStock=true`) for the board.
- **Order**: basket → `POST /shop/orders` holds the stock for **35 minutes** and opens a Stripe payment page (cards, Apple Pay, Google Pay depending on the Stripe account settings); the website sends the person there (`checkoutUrl`). One unpaid order at a time per person. Paid in time, it is confirmed by e-mail and the stock is sold; otherwise it expires and the stock is released (as it is when a member cancels). Names and prices are copied into the order: changing a price afterwards does not change it.
- **E-mails**: the buyer gets a confirmation e-mail once their order is paid; the **board** also gets a "Nouvelle commande de …" e-mail (buyer, items, total) **for every paid order** (not when a basket is filled: abandoned baskets expire quietly). Without any setting, every active board member who has signed in at least once (groups `Bureau` and `Admin`) gets it at their own address; with `Shop__NotificationEmail` (club address or mailing list), it goes to that single address. Someone invited into the board who has never signed in gets nothing but their invitation: their address is not confirmed yet, and these e-mails carry club business. A send failure never cancels the payment, and the buyer's e-mail does not depend on the board's one. **Low stock**: when a sale (paid order) brings a variant's **available** figure (shelf minus held) to its alert threshold or below (threshold set per variant, 0 = no alert), the board gets "Stock bas: product (variant)" with what is left; **only once** per drop below the threshold (nothing more until the stock goes back above it: restock, correction, cancelled order or a new threshold), and never when a basket is simply filled. Same recipients as above (`Shop__NotificationEmail` or every active board member who has signed in).
- **Handover**: the board marks a paid order as handed over in person (`deliver`).
- **Payment reliability**: two paths settle an order, Stripe's notification (webhook) and the buyer reading their order; a conditional settlement guarantees it is settled only once. On expiry, Stripe is queried before the stock is released (a late payment is not lost). A background job also releases expired orders every minute.
- **Refund**: not in the application yet, it is done from the Stripe dashboard (then, if needed, a stock correction).
- **Product image**: one image per product (JPEG, PNG or WebP, 3 MB at most), in the `shop_product_images` table (migration 022, kept separate so that lists never read the bytes; deleted with the product). The member catalogue and the board's management pages show it small.
- **Shop audit log**: the `Shop` module has its own audit log (table `shop_audit_log`, immutable, migration 021; interface `IShopAudit`), separate from the Administration one. It keeps the name of the board member who acted, the action (`product.created`, `product.updated`, `product.deleted`, `variant.created`, `variant.updated`, `variant.deleted`, `variant.restocked`, `variant.stock_corrected`, `order.delivered`), the product name or "product – variant" (copied, still readable after deletion) and a detail (quantity, old → new price…). No member data: an order handed over is only recorded by its identifier.
- **GDPR**: handing an order over keeps the **name of the board member** who did it (replaced by "Ancien membre du bureau" when their account is deleted); paid orders are accounting records of the club: they stay but become anonymous ("Compte supprimé") when the account is deleted; an unpaid order is cancelled and its stock released; the stock history keeps "Ancien membre du bureau".
- **API settings** (environment variables, never in Git): `Stripe__SecretKey` (secret key, `sk_test_…` in test, `sk_live_…` in production), `Stripe__WebhookSecret` (webhook signing secret `whsec_…`), `Shop__NotificationEmail` (optional, see E-mails) and `Shop__SiteUrl` (public address of the website, where Stripe sends the person back after payment, e.g. `https://test.badpig.fr`). Without a key, placing an order answers 500 "Le paiement en ligne n'est pas configuré".
- **Stripe webhook**: in the Stripe dashboard (test mode, then production), add an endpoint `https://<site>/api/v1/shop/payments/notifications` for the events `checkout.session.completed`, `checkout.session.async_payment_succeeded` and `checkout.session.expired`. The BFF lets this single `POST` route through without a session nor an anti-CSRF header (the signature protects it); it is not behind the Traefik access control of the test website (`/api` has its own router).
- **Stripe fees** (French cards, to be checked on Stripe's pricing page): about 1.5% + €0.25 per payment, i.e. ~€0.55 on a €20 item.
- **End-to-end tests without real money**: in Stripe test mode, the card `4242 4242 4242 4242` (future date, any CVC) succeeds; the Stripe CLI (`stripe listen --forward-to …/api/v1/shop/payments/notifications`) simulates webhooks locally.

## Club mailbox

The board writes from `contact@badpig.fr`, a mailbox hosted by the mail provider, and reads it in a real mail client (webmail or IMAP). The application does **not** replace that client: it only tells the board, on `/admin`, that new mail is waiting, next to the memberships to validate and the orders to hand over.

How it works: `IMailboxReader` (`Shared/Mailbox/`, the only place that talks to the mail server, like `Identity/` and `Payments/`) opens the inbox **read-only** over IMAP, asks for the identifiers of the unread messages and counts them. No message, sender or subject ever enters the application, nothing is stored and nothing is marked as read, so no personal data of third parties lands in the database. The answer is kept for a minute so that opening the board home page does not reconnect every time, and the server is given 8 seconds to answer.

**API settings** (environment variables, never in Git): `Mailbox__Host` (IMAP server; empty = the feature is off and the site shows nothing), `Mailbox__Port` (993 by default), `Mailbox__User` (the full address, e.g. `contact@badpig.fr`), `Mailbox__Password` (the mailbox password: a secret, it lives in the server's `.env`) and `Mailbox__WebmailUrl` (where the "Ouvrir la messagerie" link points; empty = no link). An unreachable mailbox is logged as a warning and is never an error for the board.

## Announcements: end date and automatic deletion

Every announcement has an end date (`expiresOn`, 90 days after creation by default). The day after that date it disappears for visitors and members (reading, likes and files: 404); the board still sees it with `isExpired: true` and can extend it by changing the date. A background job (every 6 h, `ExpiredAnnouncementCleanup`) then permanently deletes expired announcements, drafts included, together with their files and their likes.

## Announcements: polls

A poll is a **full announcement** (`form` = `poll`), independent from text announcements: its title is the question, its body an optional explanation, and it has its options (single or multiple answer), its anonymity and a mandatory deadline. It is created with `POST /admin/announcements/polls` ("Nouveau sondage" on the website). Only members and the board answer; they can change or withdraw their answer until the deadline, that day **included** (Paris date). The next day nothing changes any more and the results (number of answers per option) appear for members; before that they only see the number of participants. **The board sees the results at any time** (provisional until the deadline has passed). Visitors (public poll) see the question and the options, never the results. Non-anonymous poll: the board also sees who answered what, even before it closes. Anonymous poll: the application never shows who answered what (but the database keeps the member identifier so that an answer can be changed). The answers of a deleted account are erased. Migration 015 turned the announcements that had a poll attached into polls.

## Announcements: comments

Every member (and the board) sees the comments of a published announcement or poll and can write some; visitors neither see nor write comments. The conversation works **like WhatsApp**: a list of messages in arrival order, and any message can be answered, replies included; the answer shows a box with the author and the beginning of the quoted message (clicking the quote jumps to the message). If the quoted message is deleted, the answers stay and the quote reads "Message supprimé". A comment is plain text (1 to 1000 characters) that keeps its author's name. A member deletes their own comments; a board member deletes any of them. No editing. When creating and updating an announcement (or a poll), the board decides whether to **allow comments** (`commentsEnabled`, on by default, existing announcements included): closed, no comment can be added any more (409) and the website hides the section (the API still lists them, and they can still be deleted). Comments disappear with the announcement (deletion or end of display) and are erased with their author's account.

**Comment e-mails**: when a comment is added, the **author of the announcement** (or of the poll) receives an e-mail with the comment text; when it answers a message, the **author of the quoted message** also receives an e-mail with the answer and a reminder of their own message. Never an e-mail to oneself; only one when the author of the announcement is also the quoted person (the answer e-mail); nothing when the account no longer exists or is deactivated. A send failure never cancels the comment. Optional setting `Announcements__SiteUrl` (public address of the website, e.g. the test one): when it is set, the e-mails carry the `…/announcements/{id}` link to the conversation; otherwise they have no link.

## Announcements: sharing (WhatsApp, QR code) and link preview

Every published announcement has a **"Partager"** block (component `app-share-panel`, in `web/src/app/shared/ui/`, reusable by future events): a **WhatsApp** button (`https://wa.me/?text=title + link`, opens in the app or in WhatsApp Web), **Partager…** (the phone's share sheet, when the browser offers it), **Copier le lien**, and a **QR code** displayed on screen and downloadable as a PNG. The QR code is drawn in the browser (library `qrcode-generator`, no data sent to any external service). The link is the announcement page, `/announcements/{id}`, the same for everyone: an internal announcement asks the person to sign in (the website then sends them back to the page).

**Link preview**: WhatsApp, Telegram, Facebook and the like do not run the website's scripts. The BFF (`bff/src/BadPig.Bff/LinkPreview.cs`) recognises these bots (`User-Agent` header) on `/announcements/{id}` and returns the website page with the title, an excerpt of the text and the announcement poster (Open Graph tags), which produces a card with an image in the conversation. Only **public and published** announcements get this preview (the BFF queries the API without a token); an internal announcement, a draft or an expired one keeps the generic page. People's browsers are not affected. On the test environment, Traefik reserves the whole website for the board: the bots cannot reach it, so the preview cannot be seen there (it will have to be tested in production, or with a link from an unprotected site). The public address used in the tags comes from the `X-Forwarded-Proto` / `X-Forwarded-Host` headers sent by Traefik.

## Announcements: file storage

Posters and attachments are files on disk (key = GUID, no original name on disk), in the `Announcements__StoragePath` folder (`/data/announcements` in the image), mounted on a Docker volume (`badpig-test-announcements` in test, `announcements` locally). **This volume must be backed up along with the database**: the database only keeps the metadata. Videos are not hosted (YouTube / Vimeo links). The `GET /announcements*` reads are the only `/api` routes the BFF lets through without a session (the token is added when there is one); on the test environment, Traefik still reserves the whole website for the board.

## Docker images

```bash
docker build -f bff/Dockerfile -t badpig-club-web:test .   # BFF + Angular website (context: repository root)
docker build -t badpig-club-api:test api
```

The BFF (image `badpig-club-web`) reads `Oidc__Authority`, `Oidc__ClientId`, `Oidc__ClientSecret` (secret, `bff.env` file on the server) and `ReverseProxy__Clusters__api__Destinations__api__Address` (internal address of the API); the same image serves test and production. The API reads `Authentik__Issuer`, `Authentik__Audience`, `Authentik__ApiBase`, `Authentik__ApiToken`, `ConnectionStrings__Base` (PostgreSQL), `Authentik__MembersGroup`, `Authentik__BoardGroup` and `Authentik__InvitationStage` (UUID of the "badpig-invitation-email" e-mail stage). At startup it applies the scripts in `api/src/BadPig.Api/Migrations/` (never modify a script already applied: add a new one).

## Manual deployment of the test environment (before GitHub Actions)

On the VPS:

```bash
sudo mkdir -p /opt/badpig/app-test && sudo chown ubuntu: /opt/badpig/app-test
cd /opt/badpig/app-test
# copy deploy/test/compose.yml and deploy/test/.env.example here
cp .env.example .env && chmod 600 .env && nano .env      # IMAGE_*, TAG and Authentik__ApiToken
docker compose pull && docker compose up -d
docker compose ps
```

Step 25 (GitHub Actions) automates building the images (GHCR) and this deployment.

## What comes next

- Application database (`badpig_test`) and access from the API (network `db`).
- Club pages: schedule, registrations, documents.
- Production (step 27): see the list below.

## Not to be forgotten before going to production

- In the production API `.env`: `Announcements__SiteUrl=https://<public address of the website>` (no secret). Without it, comment e-mails carry no link to the conversation.
- Shop: create the **production** Stripe settings (key `sk_live_…`, production webhook endpoint and its `whsec_…`) in the production API `.env` (`Stripe__SecretKey`, `Stripe__WebhookSecret`, `Shop__SiteUrl`), enable Apple Pay / Google Pay in the Stripe dashboard, check that the `/api/v1/shop/payments/notifications` route is reachable from outside (not behind an access control) and make a real payment of a few euros to check the webhook.
- Club mailbox: set `Mailbox__Host`, `Mailbox__User`, `Mailbox__Password` and `Mailbox__WebmailUrl` in the production API `.env`, with the production mailbox.
- Test the WhatsApp link preview (it cannot be seen on the test environment, reserved for the board by Traefik) and check that the folder holding posters and attachments (`Announcements:StoragePath`) is on a backed-up volume.

## Website: design and home page

- **Design system** (designed with Claude Design, brief in `docs/design-brief.md`): everything lives in `web/src/styles.css`.
  **No colour is written anywhere but in the `:root` block**: level 1 = the palette (`--brand-base`, `--primary-base`,
  `--secondary-base`… as base / tint / shade triplets, the derived shades computed with `color-mix()`), level 2 = the roles
  (`--color-text`, `--button-primary-background`, `--badge-success-background`, `--focus-ring`…). Components only use the
  roles: **changing the club colour = changing `--brand-base`** (one line), changing a usage = changing the role's line.
- Base `1rem = 10px` (`html { font-size: 62.5% }`), text and spacing scales as variables, mobile first.
  The breakpoints (768 / 1024) are **container queries**: `app-root` carries `container-type: inline-size`, the content sits
  in a `div.vp-in` (the same structure as the mockups).
- **Self-hosted Poppins** font (`web/public/fonts`, SIL OFL licence, latin subset, weights 400 and 700): the website loads
  nothing from a CDN.
- Main utility classes: `.container`, `.card`, `.btn` (+ `-outline`, `-secondary`, `-danger`, `-ghost`, `-inverse`,
  `-sm`, `-icon`, `.is-busy`), `.highlight-*` (badges), `.alert-*` (messages), `.input` / `.field` / `.field-error`, `.table` and
  `.rtable` (a table that becomes cards below 768), `.empty`, `.skeleton`, `.stack` / `.row` / `.cols` / `.grid-auto`.
- **Lists**: `app-search-box` (field with its label and icon), `app-sort-select` (sort field + A→Z button, usable at every
  width, unlike column headers which disappear when the table becomes cards), `app-sort-header` (sortable column header, wide
  screens) and `app-pager`. A dense table uses the `.rtable` class and a `data-label` per cell: below 768 it becomes one card
  per row (`/admin/users` is the example).
- The styles of the screens not reworked yet were converted to the `1rem = 10px` base (values multiplied by 1.6): they keep
  their appearance until they are rewritten with the system classes.
- **Page kit**: every screen is built the same way — `app-page-head` (breadcrumb, title, lead), then `app-tabs` when the
  section has several pages, the messages as banners, the main action, the filters as chips, the list and its pagination. The
  rules are in `CLAUDE.md`; the component is `web/src/app/shared/ui/page-head.ts`.
- A **TRANSITION** block at the end of `styles.css` keeps the screens not reworked yet standing (old classes
  `button.secondary`, `.badge`, `input`, `table`, and the old names `--red`, `--grey`…). It will go away once every screen has
  been reworked.
- **Header and footer**: `web/src/app/shared/ui/site-chrome.ts` (`app-site-header`, `app-site-footer`). The navigation only
  keeps the sections: Accueil, Annonces, Interclubs, Boutique, Administration. **My profile and sign-out live in the account
  menu** (button with the initials, on the right). Below 1024, the sections move into a panel opened by the Menu button, which
  also carries the account. The **members area** is gone: its content is the dashboard on the home page (the `site` module no
  longer exists).
- **Consistency between screens**: heading sizes come from the system (the page `h1` at `--text-h2`, a page's `h2`/`h3`/`h4`
  one step below), a list's filters sit in a `div.filters`, secondary text is `.small-text`, and a destructive action is always
  a `.btn-danger` (raspberry) so that it stands out from the others. No screen redefines these styles locally.
- **Navigation inside a section**: `app-tabs` (`shared/ui/tabs.ts`) shows a section's pages as chips, the current one marked;
  for the interclubs the list is in `modules/interclubs/core/tabs.ts` (Calendrier, Disponibilités, Équipes).
- **Administration**: `/admin` is the board's home page (`web/src/app/admin/admin-home.page.ts`), with the areas grouped by
  topic (members and memberships, communication, interclubs, shop, audit log) and, at the top, one reminder per thing to do:
  the memberships waiting to be validated and the paid orders waiting to be handed over (counted by `ShopAlerts`, the only
  entry point of the Shop module open to the host). The member list, the invitations and the actions on an account are on
  `/admin/users`.
- **"Le club" page** (`web/src/app/club/club.page.ts`, route `/club`, public, in the navigation between Accueil and Annonces):
  presentation of the association, practical information and a **Le bureau** section. A **visitor** only sees the president
  there (the club's public contact) and a reminder that the board's contact details are reserved for members; a **signed-in
  member** sees the whole board, each person with their title and a "Écrire" link, read from `/club/board` (no name is written
  in the code: the page follows the accounts of the `Bureau` group and the titles managed in the administration).
- **Social networks**: the Facebook (`https://www.facebook.com/BadPig34/`) and Instagram
  (`https://www.instagram.com/badminton_pignan/`) links are in the footer and in the contact block of the home page.
- **Home page**: `web/src/app/home/home.page.ts`, in the host (it assembles blocks from several modules, which a module is not
  allowed to do). Visitor: a hero (which also introduces the club), then two columns — the three latest public announcements on
  one side, the sessions, the place and the contact on the other (stacked on a small screen, announcements first).
  The website never says how to become a member: people join the club on the board's invitation, away from the website.
  Member in good standing: announcements, next fixtures (those of their teams first, marked "Mon équipe"), date proposals
  waiting for their answer, shortcuts, and a reminder when their fee is not validated. The blocks come from the modules:
  `app-latest-announcements` (Announcements), `app-next-fixtures` and `app-pending-answers` (Interclubs).

## Website: interclubs

- Members (**Interclubs** menu link, section navigation through `app-tabs`: Calendrier, Disponibilités, Équipes):
  `/interclubs` (calendar of every team: period and team filters as chips, search; captains add their fixtures there),
  `/interclubs/fixtures/:id` (details, update, delete for the captain and the board),
  `/interclubs/teams` (**overview**: one card per team with its captain, its squad and its **next fixture**;
  the board creates the teams and appoints the captains there, under "Gérer"), **`/interclubs/teams/:id` (the team page**:
  identity, captain actions, next fixtures, proposed dates, squad and latest fixtures),
  `/interclubs/teams/:id/members` (squad: the captain or the board picks the players among all active members).
  **The date proposals are hidden**: the API and the pages still exist (`ListDateProposals`, `/interclubs/proposals`,
  `/interclubs/proposals/:id`), but the tab, the routes and the "À répondre" block of the home page are commented out
  (`modules/interclubs/core/tabs.ts`, `interclubs.routes.ts`). To bring them back: uncomment those three lines. Fixtures are
  created directly with "Ajouter une rencontre".
- Board: on `/interclubs/teams`, create / update / delete a team and appoint or remove its captain.
- On a fixture page, the captain or the board sets the lineup (7 matches, players from the squad) and enters the results from
  the day of the fixture onwards (the winner of each match and the fixture score are computed): **match by match** with the
  button on each row (as the evening goes) or all 7 matches at once. The calendar shows the final score, and the progress
  ("3 – 2, 5 matchs sur 7") while the results are being entered, with the "Rencontres en cours" filter.
- **"En direct" banner** (`app-live-fixtures`, placed by `app.ts` under the header, for members in good standing): when a
  fixture is being played right now (see `when=live`), it shows the team, the opponent, the score as it goes and a link to the
  fixture; it refreshes every minute and stays invisible the rest of the time.
- Not yet: pool standings, interclubs audit log.

## Website: shop

- Members (validated or within the grace period, **Boutique** menu link): `/shop` (catalogue, basket, Stripe payment), `/shop/orders` (my orders), `/shop/orders/:id` (details, pay, cancel).
- Board: `/admin/shop` (products, variants, stock, low-stock alerts), `/admin/shop/orders` (incoming orders, handover in person), `/admin/shop/variants/:id/history` (movement history). The three pages share the `app-tabs` navigation (Articles et stock, Commandes, Journal). On the products page, each variant fits on one row (label, price, green / amber / raspberry stock chip) with a single visible action, **Réapprovisionner**; correcting the stock, updating, history and deleting are under "Plus d'actions", and the product actions under "Gérer l'article".
- Board: `/admin/shop/audit-log` (shop audit log: who did what, when).
