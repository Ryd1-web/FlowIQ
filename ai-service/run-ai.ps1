param(
    [int]$Port = 8000
)

$ErrorActionPreference = "Stop"

Set-Location -Path $PSScriptRoot

if (-not (Test-Path ".venv")) {
    python -m venv .venv
}

& .\.venv\Scripts\python -m pip install --upgrade pip
& .\.venv\Scripts\python -m pip install -r requirements.txt
& .\.venv\Scripts\python -m uvicorn main:app --host 0.0.0.0 --port $Port --reload
