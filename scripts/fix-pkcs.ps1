$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$pkg  = 'System.Security.Cryptography.Pkcs'

dotnet restore "$root\InvoiceTracker.slnx" | Out-Null
$report = dotnet list "$root\InvoiceTracker.slnx" package --vulnerable --include-transitive --format json | ConvertFrom-Json

$projects = $report.projects | Where-Object {
    $_.frameworks | Where-Object {
        ($_.topLevelPackages + $_.transitivePackages) | Where-Object { $_.id -eq $pkg }
    }
} | Select-Object -ExpandProperty path

if (-not $projects) {
    Write-Host "No project references a vulnerable $pkg." -ForegroundColor Green
    return
}

foreach ($p in $projects) {
    Write-Host "==> $p" -ForegroundColor Cyan
    dotnet add $p package $pkg
    if ($LASTEXITCODE -ne 0) { throw "Failed on $p" }
}

dotnet list "$root\InvoiceTracker.slnx" package --vulnerable --include-transitive
