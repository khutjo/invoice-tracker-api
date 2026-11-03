# Build context is the repo root: docker build -f src/Services/Identity/InvoiceTracker.Identity.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore from project files only, so this layer stays cached until a csproj changes
COPY global.json Directory.Build.props .editorconfig ./
COPY ["src/Services/Identity/InvoiceTracker.Identity.Api/InvoiceTracker.Identity.Api.csproj", "src/Services/Identity/InvoiceTracker.Identity.Api/"]
COPY ["src/Services/Identity/InvoiceTracker.Identity.Infrastructure/InvoiceTracker.Identity.Infrastructure.csproj", "src/Services/Identity/InvoiceTracker.Identity.Infrastructure/"]
COPY ["src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Contracts/InvoiceTracker.BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Contracts/"]
COPY ["src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Messaging/InvoiceTracker.BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Messaging/"]
COPY ["src/BuildingBlocks/InvoiceTracker.BuildingBlocks.ServiceDefaults/InvoiceTracker.BuildingBlocks.ServiceDefaults.csproj", "src/BuildingBlocks/InvoiceTracker.BuildingBlocks.ServiceDefaults/"]
RUN dotnet restore "src/Services/Identity/InvoiceTracker.Identity.Api/InvoiceTracker.Identity.Api.csproj"

COPY src/ src/
RUN dotnet publish "src/Services/Identity/InvoiceTracker.Identity.Api/InvoiceTracker.Identity.Api.csproj" -c Release -o /app/publish --no-restore -p:UseAppHost=false

# Chiseled: no shell, non-root by default, listens on 8080. -extra adds ICU and tzdata (Africa/Johannesburg)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InvoiceTracker.Identity.Api.dll"]
