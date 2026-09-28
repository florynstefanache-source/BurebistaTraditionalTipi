param([Parameter(Mandatory = $true)][string]$TLDPath)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
if (!(Test-Path -LiteralPath (Join-Path $TLDPath 'MelonLoader'))) {
    throw 'Selecciona la carpeta de The Long Dark con MelonLoader instalado.'
}
$sourceMods = Join-Path $repoRoot 'release/Mods'
$targetMods = Join-Path $TLDPath 'Mods'
New-Item -ItemType Directory -Force -Path $targetMods | Out-Null
Get-ChildItem -LiteralPath $sourceMods | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $targetMods -Recurse -Force
}
Write-Host 'Instalados: Tipi 1.2.3, Curing Rack 0.1.1 y Workbench 0.3.4. Las dependencias se instalan por separado.'
