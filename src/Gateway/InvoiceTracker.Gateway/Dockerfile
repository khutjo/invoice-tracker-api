# Build context is the repo root: docker build -f src/Gateway/InvoiceTracker.Gateway/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore from project files only, so this layer stays cached until a csproj changes
COPY global.json Directory.Build.props .editorconfig ./
COPY ["src/Gateway/InvoiceTracker.Gateway/InvoiceTracker.Gateway.csproj", "src/Gateway/InvoiceTracker.Gateway/"]
COPY ["src/BuildingBlocks/InvoiceTracker.BuildingBlocks.ServiceDefaults/InvoiceTracker.BuildingBlocks.ServiceDefaults.csproj", "src/BuildingBlocks/InvoiceTracker.BuildingBlocks.ServiceDefaults/"]
RUN dotnet restore "src/Gateway/InvoiceTracker.Gateway/InvoiceTracker.Gateway.csproj"

COPY src/ src/
RUN dotnet publish "src/Gateway/InvoiceTracker.Gateway/InvoiceTracker.Gateway.csproj" -c Release -o /app/publish --no-restore -p:UseAppHost=false

# Chiseled: no shell, non-root by default, listens on 8080. -extra adds ICU and tzdata (Africa/Johannesburg)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InvoiceTracker.Gateway.dll"]
