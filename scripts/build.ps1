param([string]$GameDir = 'C:\Program Files (x86)\Steam\steamapps\common\TheLongDark')
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
$projects = @(
    @('BurebistaTraditionalTipi.csproj', 'Tipi'),
    @('addons/TipiCuringRack/Source/TipiCuringRack.csproj', 'TipiCuringRack'),
    @('addons/TipiWorkbench/Source/TipiWorkbench.csproj', 'TipiWorkbench')
)
foreach ($project in $projects) {
    dotnet build (Join-Path $repoRoot $project[0]) -c Release "-p:GameDir=$GameDir" -o (Join-Path $repoRoot ('artifacts/' + $project[1]))
    if ($LASTEXITCODE -ne 0) { throw "Error al compilar $($project[1])" }
}
