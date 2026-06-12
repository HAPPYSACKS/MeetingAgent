[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $BaseUrl,

    [Parameter(Mandatory = $true)]
    [guid] $TeamsAppId,

    [Parameter(Mandatory = $true)]
    [guid] $EntraClientId,

    [Parameter(Mandatory = $false)]
    [string] $OutputDirectory = "artifacts/teams"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$templatePath = Join-Path $repoRoot "teams/appPackage/manifest.template.json"
$colorIconPath = Join-Path $repoRoot "teams/appPackage/color.png"
$outlineIconPath = Join-Path $repoRoot "teams/appPackage/outline.png"
$resolvedOutputDirectory = if ([System.IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory
} else {
    Join-Path $repoRoot $OutputDirectory
}
$packageWorkDirectory = Join-Path $resolvedOutputDirectory "package"
$manifestPath = Join-Path $packageWorkDirectory "manifest.json"
$packagePath = Join-Path $resolvedOutputDirectory "MeetingAgent.TeamsApp.zip"

$baseUri = $null
if (-not [Uri]::TryCreate($BaseUrl, [UriKind]::Absolute, [ref] $baseUri)) {
    throw "BaseUrl must be an absolute HTTPS URL."
}

if ($baseUri.Scheme -ne "https") {
    throw "BaseUrl must use HTTPS because Teams requires secure tab URLs."
}

$normalizedBaseUrl = $baseUri.GetLeftPart([UriPartial]::Authority).TrimEnd("/")
$devTunnelHost = $baseUri.Host

foreach ($requiredPath in @($templatePath, $colorIconPath, $outlineIconPath)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Missing Teams app package source file: $requiredPath"
    }
}

New-Item -ItemType Directory -Force -Path $packageWorkDirectory | Out-Null

$manifest = Get-Content -Raw -LiteralPath $templatePath
$manifest = $manifest.Replace("{{BaseUrl}}", $normalizedBaseUrl)
$manifest = $manifest.Replace("{{TeamsAppId}}", $TeamsAppId.ToString())
$manifest = $manifest.Replace("{{EntraClientId}}", $EntraClientId.ToString())
$manifest = $manifest.Replace("{{DevTunnelHost}}", $devTunnelHost)

$manifest | ConvertFrom-Json | Out-Null
[System.IO.File]::WriteAllText($manifestPath, $manifest, [System.Text.UTF8Encoding]::new($false))
Copy-Item -LiteralPath $colorIconPath -Destination (Join-Path $packageWorkDirectory "color.png") -Force
Copy-Item -LiteralPath $outlineIconPath -Destination (Join-Path $packageWorkDirectory "outline.png") -Force

if (Test-Path -LiteralPath $packagePath) {
    Remove-Item -LiteralPath $packagePath -Force
}

Compress-Archive -Path (Join-Path $packageWorkDirectory "*") -DestinationPath $packagePath

Write-Host "Teams app package created: $packagePath"
