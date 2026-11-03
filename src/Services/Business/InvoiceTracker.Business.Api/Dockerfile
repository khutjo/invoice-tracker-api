# Build context is the repo root: docker build -f src/Services/Business/InvoiceTracker.Business.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore from project files only, so this layer stays cached until a csproj changes
COPY global.json Directory.Build.props .editorconfig ./
COPY ["src/Services/Business/InvoiceTracker.Business.Api/InvoiceTracker.Business.Api.csproj", "src/Services/Business/InvoiceTracker.Business.Api/"]
COPY ["src/Services/Business/InvoiceTracker.Business.Application/InvoiceTracker.Business.Application.csproj", "src/Services/Business/InvoiceTracker.Business.Application/"]
COPY ["src/Services/Business/InvoiceTracker.Business.Domain/InvoiceTracker.Business.Domain.csproj", "src/Services/Business/InvoiceTracker.Business.Domain/"]
COPY ["src/Services/Business/InvoiceTracker.Business.Infrastructure/InvoiceTracker.Business.Infrastructure.csproj", "src/Services/Business/InvoiceTracker.Business.Infrastructure/"]
COPY ["src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Contracts/InvoiceTracker.BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Contracts/"]
COPY ["src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Messaging/InvoiceTracker.BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Messaging/"]
COPY ["src/BuildingBlocks/InvoiceTracker.BuildingBlocks.ServiceDefaults/InvoiceTracker.BuildingBlocks.ServiceDefaults.csproj", "src/BuildingBlocks/InvoiceTracker.BuildingBlocks.ServiceDefaults/"]
RUN dotnet restore "src/Services/Business/InvoiceTracker.Business.Api/InvoiceTracker.Business.Api.csproj"

COPY src/ src/
RUN dotnet publish "src/Services/Business/InvoiceTracker.Business.Api/InvoiceTracker.Business.Api.csproj" -c Release -o /app/publish --no-restore -p:UseAppHost=false

# Logo folder owned by the non-root app user (1654), so the named volume mounted here starts writable
RUN mkdir -p /logos

# Chiseled: no shell, non-root by default, listens on 8080. -extra adds ICU and tzdata (Africa/Johannesburg)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build --chown=1654:1654 /logos /app/App_Data/logos
ENTRYPOINT ["dotnet", "InvoiceTracker.Business.Api.dll"]
