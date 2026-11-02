# --- Build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# Project files first: the NuGet restore layer is only rebuilt when a dependency changes.
COPY src/BadPig.Api/BadPig.Api.csproj src/BadPig.Api/
RUN dotnet restore src/BadPig.Api/BadPig.Api.csproj
COPY src/ src/
RUN dotnet publish src/BadPig.Api/BadPig.Api.csproj -c Release -o /app --no-restore /p:UseAppHost=false

# --- Runtime (unprivileged user, port 8080) ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_gcServer=0 \
    DOTNET_GCHeapHardLimit=0x7000000
# Folder of the announcements' files (poster, attachments): a volume in the compose files, writable by the unprivileged user.
ENV Announcements__StoragePath=/data/announcements
RUN mkdir -p /data/announcements && chown $APP_UID:$APP_UID /data/announcements
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "BadPig.Api.dll"]
