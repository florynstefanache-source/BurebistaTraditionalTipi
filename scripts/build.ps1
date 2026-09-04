param(
    [string]$TLDPath = "C:\Program Files (x86)\Steam\steamapps\common\TheLongDark"
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$outputPath = Join-Path $projectRoot "artifacts"

dotnet build (Join-Path $projectRoot "BurebistaTraditionalTipi.csproj") `
    -c Release `
    -p:TLDPath="$TLDPath" `
    -o $outputPath

Write-Host "Build complete: $outputPath"

