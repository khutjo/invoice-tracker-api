<#
.SYNOPSIS
    Plays the payment processor: sends a signed "payment.succeeded" webhook for an invoice.

.EXAMPLE
    .\scripts\simulate-payment.ps1 -InvoiceId 0193... -Amount 500.00

.EXAMPLE
    # Send the same event twice to see the second one answered as a duplicate
    .\scripts\simulate-payment.ps1 -InvoiceId 0193... -Amount 500.00 -Replay
#>
param(
    [Parameter(Mandatory = $true)]
    [Guid] $InvoiceId,

    [Parameter(Mandatory = $true)]
    [decimal] $Amount,

    # Same value as PAYMENT_WEBHOOK_SECRET in .env (or PaymentWebhook:Secret in user-secrets)
    [string] $Secret = $env:PAYMENT_WEBHOOK_SECRET,

    [string] $BaseUrl = 'http://localhost:5165',

    [string] $Reference,

    [ValidateSet('eft', 'card', 'cash', 'other')]
    [string] $Method = 'eft',

    [string] $EventId = "evt_$([Guid]::NewGuid().ToString('N'))",

    [switch] $Replay
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($Secret)) {
    # Fall back to the value in the repo's .env so the demo needs no extra setup
    $envFile = Join-Path $PSScriptRoot '..\.env'
    if (Test-Path $envFile) {
        $line = Get-Content $envFile | Where-Object { $_ -match '^PAYMENT_WEBHOOK_SECRET=' } | Select-Object -First 1
        if ($line) {
            $Secret = $line.Substring('PAYMENT_WEBHOOK_SECRET='.Length).Trim()
        }
    }
}

if ([string]::IsNullOrWhiteSpace($Secret)) {
    throw 'No webhook secret. Pass -Secret, set $env:PAYMENT_WEBHOOK_SECRET, or add PAYMENT_WEBHOOK_SECRET to .env.'
}

$payload = [ordered]@{
    eventId   = $EventId
    eventType = 'payment.succeeded'
    invoiceId = $InvoiceId.ToString()
    amount    = $Amount
    currency  = 'ZAR'
    paidAt    = [DateTimeOffset]::UtcNow.ToString('o')
    method    = $Method
}
if ($Reference) {
    $payload.reference = $Reference
}

$body = $payload | ConvertTo-Json -Compress

function Send-Webhook {
    # Signed fresh each time: the timestamp is part of the signature
    $timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds().ToString()
    $hmac = [System.Security.Cryptography.HMACSHA256]::new([System.Text.Encoding]::UTF8.GetBytes($Secret))
    try {
        $hash = $hmac.ComputeHash([System.Text.Encoding]::UTF8.GetBytes("$timestamp.$body"))
    }
    finally {
        $hmac.Dispose()
    }
    $signature = 'v1=' + (-join ($hash | ForEach-Object { $_.ToString('x2') }))

    Invoke-RestMethod -Method Post `
        -Uri "$BaseUrl/api/v1/webhooks/payments" `
        -ContentType 'application/json; charset=utf-8' `
        -Headers @{ 'Webhook-Timestamp' = $timestamp; 'Webhook-Signature' = $signature } `
        -Body ([System.Text.Encoding]::UTF8.GetBytes($body))
}

Write-Host "POST $BaseUrl/api/v1/webhooks/payments"
Write-Host $body
Send-Webhook | Format-List

if ($Replay) {
    Write-Host 'Replaying the same event...'
    Send-Webhook | Format-List
}
