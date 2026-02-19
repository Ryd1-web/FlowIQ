param(
    [string]$BaseUrl = "http://localhost:8000"
)

$ErrorActionPreference = "Stop"

Set-Location -Path $PSScriptRoot

$recordDir = Join-Path $PSScriptRoot "logs"
if (-not (Test-Path $recordDir)) {
    New-Item -ItemType Directory -Path $recordDir | Out-Null
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$recordFile = Join-Path $recordDir "ai-smoke-$timestamp.json"

$predictPayload = @{
    business_id = "11111111-1111-1111-1111-111111111111"
    incomes = @(
        @{ amount = 20000; date = "2026-02-10"; label = ""; category = $null },
        @{ amount = 30000; date = "2026-02-11"; label = ""; category = $null }
    )
    expenses = @(
        @{ amount = 25000; date = "2026-02-10"; label = ""; category = $null },
        @{ amount = 26000; date = "2026-02-11"; label = ""; category = $null }
    )
    prediction_days = 30
}

$anomalyPayload = @{
    business_id = "11111111-1111-1111-1111-111111111111"
    incomes = @(
        @{ amount = 20000; date = "2026-02-10"; label = ""; category = $null },
        @{ amount = 30000; date = "2026-02-11"; label = ""; category = $null }
    )
    expenses = @(
        @{ amount = 25000; date = "2026-02-10"; label = ""; category = $null },
        @{ amount = 26000; date = "2026-02-11"; label = ""; category = $null }
    )
}

$categorizePayload = @{
    text = "Fuel purchase and office supplies"
}

$health = Invoke-RestMethod -Method Get -Uri "$BaseUrl/health"
$predict = Invoke-RestMethod -Method Post -Uri "$BaseUrl/predict/cashflow" -ContentType "application/json" -Body ($predictPayload | ConvertTo-Json -Depth 8)
$anomaly = Invoke-RestMethod -Method Post -Uri "$BaseUrl/detect/anomaly" -ContentType "application/json" -Body ($anomalyPayload | ConvertTo-Json -Depth 8)
$categorize = Invoke-RestMethod -Method Post -Uri "$BaseUrl/categorize/receipt" -ContentType "application/json" -Body ($categorizePayload | ConvertTo-Json -Depth 8)

$record = @{
    timestamp = (Get-Date).ToString("o")
    baseUrl = $BaseUrl
    health = $health
    predict = $predict
    anomaly = $anomaly
    categorize = $categorize
}

$record | ConvertTo-Json -Depth 12 | Set-Content -Path $recordFile
Write-Host "Smoke test record saved to: $recordFile"
