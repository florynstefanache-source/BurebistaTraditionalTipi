param(
    [Parameter(Mandatory = $true)]
    [string]$TLDPath
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$releaseZip = Join-Path $projectRoot "release\BurebistaTraditionalTipi-v1.1.zip"

if (-not (Test-Path -LiteralPath $releaseZip)) {
    throw "Release package not found: $releaseZip"
}

Expand-Archive -LiteralPath $releaseZip -DestinationPath $TLDPath -Force
Write-Host "Burebista Traditional Tipi installed in: $TLDPath"

