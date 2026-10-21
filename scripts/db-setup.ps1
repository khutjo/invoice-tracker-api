# Starts SQL Server, waits for the app login, stores per-service connection strings
# in user-secrets and applies every service's EF Core migrations.
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent

$envFile = Join-Path $root '.env'
if (-not (Test-Path $envFile)) { throw ".env not found at $envFile (copy .env.example)" }
$line = Get-Content $envFile | Where-Object { $_ -match '^\s*APP_DB_PASSWORD\s*=' } | Select-Object -First 1
$password = ($line -replace '^\s*APP_DB_PASSWORD\s*=', '').Trim().Trim('"', "'")
if (-not $password) { throw 'APP_DB_PASSWORD not found in .env' }

Push-Location $root
try {
    docker compose up -d
    if ($LASTEXITCODE -ne 0) { throw 'docker compose up failed' }

    # Blocks until the one-shot init container exits, then reads its exit code.
    docker compose wait sqlserver-init | Out-Null
    $initId   = docker compose ps -a -q sqlserver-init
    $initExit = docker inspect -f '{{.State.ExitCode}}' $initId
    if ($initExit -ne '0') {
        docker compose logs sqlserver-init
        throw 'sqlserver-init failed'
    }
}
finally { Pop-Location }

$services = @(
    @{ Name = 'Identity';     Key = 'IdentityDb';     Db = 'identity_db' },
    @{ Name = 'Business';     Key = 'BusinessDb';     Db = 'business_db' },
    @{ Name = 'Invoicing';    Key = 'InvoicingDb';    Db = 'invoicing_db' },
    @{ Name = 'Notification'; Key = 'NotificationDb'; Db = 'notification_db' }
)

foreach ($s in $services) {
    $api   = "$root\src\Services\$($s.Name)\InvoiceTracker.$($s.Name).Api"
    $infra = "$root\src\Services\$($s.Name)\InvoiceTracker.$($s.Name).Infrastructure"
    # Password is quoted so characters like ';' cannot break the connection string.
    $conn  = "Server=localhost,1433;Database=$($s.Db);User Id=InvoiceTrackerUser;Password='$password';TrustServerCertificate=True"

    Write-Host "==> $($s.Name)" -ForegroundColor Cyan
    dotnet user-secrets set "ConnectionStrings:$($s.Key)" $conn --project $api | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "user-secrets failed for $($s.Name)" }
    dotnet ef database update --project $infra --startup-project $api
    if ($LASTEXITCODE -ne 0) { throw "Migration failed for $($s.Name)" }
}

Write-Host 'All databases migrated.' -ForegroundColor Green
