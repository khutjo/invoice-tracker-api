# Build context is the repo root: docker build -f src/Services/Invoicing/InvoiceTracker.Invoicing.Api/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore from project files only, so this layer stays cached until a csproj changes
COPY global.json Directory.Build.props .editorconfig ./
COPY ["src/Services/Invoicing/InvoiceTracker.Invoicing.Api/InvoiceTracker.Invoicing.Api.csproj", "src/Services/Invoicing/InvoiceTracker.Invoicing.Api/"]
COPY ["src/Services/Invoicing/InvoiceTracker.Invoicing.Application/InvoiceTracker.Invoicing.Application.csproj", "src/Services/Invoicing/InvoiceTracker.Invoicing.Application/"]
COPY ["src/Services/Invoicing/InvoiceTracker.Invoicing.Domain/InvoiceTracker.Invoicing.Domain.csproj", "src/Services/Invoicing/InvoiceTracker.Invoicing.Domain/"]
COPY ["src/Services/Invoicing/InvoiceTracker.Invoicing.Infrastructure/InvoiceTracker.Invoicing.Infrastructure.csproj", "src/Services/Invoicing/InvoiceTracker.Invoicing.Infrastructure/"]
COPY ["src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Contracts/InvoiceTracker.BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Contracts/"]
COPY ["src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Messaging/InvoiceTracker.BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/InvoiceTracker.BuildingBlocks.Messaging/"]
COPY ["src/BuildingBlocks/InvoiceTracker.BuildingBlocks.ServiceDefaults/InvoiceTracker.BuildingBlocks.ServiceDefaults.csproj", "src/BuildingBlocks/InvoiceTracker.BuildingBlocks.ServiceDefaults/"]
RUN dotnet restore "src/Services/Invoicing/InvoiceTracker.Invoicing.Api/InvoiceTracker.Invoicing.Api.csproj"

COPY src/ src/
RUN dotnet publish "src/Services/Invoicing/InvoiceTracker.Invoicing.Api/InvoiceTracker.Invoicing.Api.csproj" -c Release -o /app/publish --no-restore -p:UseAppHost=false

# Chiseled: no shell, non-root by default, listens on 8080. -extra adds ICU and tzdata (Africa/Johannesburg)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InvoiceTracker.Invoicing.Api.dll"]
