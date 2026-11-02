# Run BadPig locally (Docker)

## Prerequisites
- Docker Desktop.
- The OIDC client secret and the Authentik API token of the test environment (put them only in `deploy/local/.env`, ignored by Git).

## One-time setup in Authentik (test)
Provider "Provider for BadPig test (site web)" → **Redirect URIs/Origins**, add (strict mode):
- `http://localhost:8080/signin-oidc`
- `http://localhost:8080/signout-callback-oidc`

## Start
```powershell
cd deploy\local
copy .env.example .env      # then fill in the 2 secrets
docker compose up --build
```
Open http://localhost:8080 and sign in with your test account (the Bureau/Admin groups apply as on test).

## Useful commands
```powershell
docker compose logs -f api web      # logs
docker compose up --build -d api    # rebuild only the API after a change
docker compose down                 # stop (data kept)
docker compose down -v              # stop and erase the local database
```
Database: `localhost:5432`, db `badpig_local`, user `badpig`, password `badpig_local`. Migrations are applied at API start.

## Limits
- Identity is the test Authentik: invitations, (de)activation, board changes and deletions made locally really modify the test accounts.
- No SMTP locally: e-mails are not sent.

## Swagger UI
Sign in on http://localhost:8080 first (with a Bureau account), then open:

**http://localhost:8080/api/v1/swagger**

Swagger UI is served by the API in Development only (never on test/production). It goes through the site (BFF): your session adds the token and the anti-CSRF header to every "Try it out" call. The OpenAPI document is at `/api/v1/openapi/v1.json`.
