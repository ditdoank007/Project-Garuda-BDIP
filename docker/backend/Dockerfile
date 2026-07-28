# ============================
# Build Stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY backend/ ./backend/

WORKDIR /src/backend

RUN dotnet restore BDIP.sln

RUN dotnet publish API/BDIP.API.csproj \
    -c Release \
    -o /app/publish

# ============================
# Runtime Stage
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Install native LDAP libraries
RUN apt-get update && \
    apt-get install -y \
        libldap-2.5-0 \
        libsasl2-2 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "BDIP.API.dll"]