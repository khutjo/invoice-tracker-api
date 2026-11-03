# Build context is the repo root: docker build -f src/Services/Notification/InvoiceTracker.Notification.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore from project files only, so this layer stays cached until a csproj changes
COPY global.json Directory.Build.props .editorconfig ./
COPY ["src/Services/Notification/InvoiceTracker.Notification.Api/InvoiceTracker.Notification.Api.csproj", "src/Services/Notification/InvoiceTracker.Notification.Api/"]
COPY ["src/Services/Notification/InvoiceTracker.Notification.Infrastructure/InvoiceTracker.Notification.Infrastructure.csproj", "src/Services/Notification/InvoiceTracker.Notification.Infrastructure/"]
COPY ["src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Contracts/InvoiceTracker.BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Contracts/"]
COPY ["src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Messaging/InvoiceTracker.BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Messaging/"]
COPY ["src/BuildingBlocks/InvoiceTracker.BuildingBlocks.ServiceDefaults/InvoiceTracker.BuildingBlocks.ServiceDefaults.csproj", "src/BuildingBlocks/InvoiceTracker.BuildingBlocks.ServiceDefaults/"]
RUN dotnet restore "src/Services/Notification/InvoiceTracker.Notification.Api/InvoiceTracker.Notification.Api.csproj"

COPY src/ src/
RUN dotnet publish "src/Services/Notification/InvoiceTracker.Notification.Api/InvoiceTracker.Notification.Api.csproj" -c Release -o /app/publish --no-restore -p:UseAppHost=false

# Chiseled: no shell, non-root by default, listens on 8080. -extra adds ICU and tzdata (Africa/Johannesburg)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InvoiceTracker.Notification.Api.dll"]
