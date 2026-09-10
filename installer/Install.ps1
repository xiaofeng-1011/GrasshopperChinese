param([string]$LibrariesPath = (Join-Path $env:APPDATA 'Grasshopper\Libraries'))
$ErrorActionPreference = 'Stop'
if (Get-Process Rhino -ErrorAction SilentlyContinue) { throw 'Close Rhino before installing.' }
$payload = Join-Path $PSScriptRoot 'GrasshopperChinese'
if (!(Test-Path "$payload\GrasshopperChinese.gha")) {
    $payload = Join-Path (Split-Path $PSScriptRoot -Parent) 'build\GrasshopperChinese'
}
if (!(Test-Path "$payload\GrasshopperChinese.gha")) { throw 'Package payload missing. Extract the complete ZIP first.' }
$destination = Join-Path $LibrariesPath 'GrasshopperChinese'
New-Item -ItemType Directory -Force -Path $destination | Out-Null
foreach ($file in Get-ChildItem -LiteralPath $payload -File -Recurse) {
    $relative = $file.FullName.Substring($payload.Length).TrimStart('\')
    $target = Join-Path $destination $relative
    New-Item -ItemType Directory -Force -Path (Split-Path $target -Parent) | Out-Null
    Copy-Item -LiteralPath $file.FullName -Destination $target -Force
    Unblock-File -LiteralPath $target
}
Write-Host "Installed to $destination. Start Rhino 8 and Grasshopper."
