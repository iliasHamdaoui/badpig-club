# Single image of the site: the BFF (ASP.NET Core) which serves the Angular application.
# Build context: the repository root (docker build -f bff/Dockerfile .)

# --- Build of the Angular site ---
FROM node:22-alpine AS site
WORKDIR /src
COPY web/package.json web/package-lock.json ./
RUN npm ci
COPY web/ .
RUN node scripts/check-module-boundaries.mjs && npx ng build --configuration production

# --- Build of the BFF ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS bff
WORKDIR /src
COPY bff/src/BadPig.Bff/BadPig.Bff.csproj bff/src/BadPig.Bff/
RUN dotnet restore bff/src/BadPig.Bff/BadPig.Bff.csproj
COPY bff/src/ bff/src/
RUN dotnet publish bff/src/BadPig.Bff/BadPig.Bff.csproj -c Release -o /app --no-restore /p:UseAppHost=false

# --- Runtime (unprivileged user, port 8080) ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=bff /app .
COPY --from=site /src/dist/web/browser/ wwwroot/
# Behind Traefik: the X-Forwarded-* headers are trusted (public https address, needed for the OIDC callbacks).
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    DOTNET_gcServer=0 \
    DOTNET_GCHeapHardLimit=0x4000000
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "BadPig.Bff.dll"]
