param(
    [int]$Port = 8000
)

$ErrorActionPreference = "Stop"

Set-Location -Path $PSScriptRoot

$logDir = Join-Path $PSScriptRoot "logs"
if (-not (Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir | Out-Null
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$logFile = Join-Path $logDir "ai-service-$timestamp.log"

if (-not (Test-Path ".venv")) {
    python -m venv .venv
}

& .\.venv\Scripts\python -m pip install --upgrade pip
& .\.venv\Scripts\python -m pip install -r requirements.txt

Write-Host "Writing AI service logs to: $logFile"
& .\.venv\Scripts\python -m uvicorn main:app --host 0.0.0.0 --port $Port --reload 2>&1 | Tee-Object -FilePath $logFile
