Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$envPath = Join-Path $repoRoot ".env"

if (Test-Path -LiteralPath $envPath) {
    Get-Content -LiteralPath $envPath | ForEach-Object {
        $line = $_.Trim()
        if ($line.Length -eq 0 -or $line.StartsWith("#")) {
            return
        }

        $separatorIndex = $line.IndexOf("=")
        if ($separatorIndex -lt 1) {
            return
        }

        $name = $line.Substring(0, $separatorIndex).Trim()
        if (-not [string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name, "Process"))) {
            return
        }

        $value = $line.Substring($separatorIndex + 1).Trim().Trim('"').Trim("'")
        [Environment]::SetEnvironmentVariable($name, $value, "Process")
    }
}

$baseUrl = $env:MEETINGAGENT_TEAMS_BASE_URL
if ([string]::IsNullOrWhiteSpace($baseUrl)) {
    throw "Set MEETINGAGENT_TEAMS_BASE_URL in .env or your shell, for example https://<your-dev-tunnel-host>."
}

$teamsAppId = if ([string]::IsNullOrWhiteSpace($env:MEETINGAGENT_TEAMS_APP_ID)) {
    [guid]::NewGuid()
} else {
    [guid]::Parse($env:MEETINGAGENT_TEAMS_APP_ID)
}

$packageArgs = @{
    BaseUrl = $baseUrl
    TeamsAppId = $teamsAppId
}

$includePreviewAuth = $env:MEETINGAGENT_INCLUDE_PREVIEW_AUTH -in @("1", "true", "True", "TRUE", "yes", "Yes", "YES")
if ($includePreviewAuth) {
    if ([string]::IsNullOrWhiteSpace($env:MEETINGAGENT_ENTRA_CLIENT_ID)) {
        throw "Set MEETINGAGENT_ENTRA_CLIENT_ID when MEETINGAGENT_INCLUDE_PREVIEW_AUTH=true."
    }

    $packageArgs.EntraClientId = [guid]::Parse($env:MEETINGAGENT_ENTRA_CLIENT_ID)
    $packageArgs.IncludePreviewAuth = $true
}

& (Join-Path $repoRoot "scripts/New-TeamsAppPackage.ps1") @packageArgs
Write-Host "Teams app id: $teamsAppId"
