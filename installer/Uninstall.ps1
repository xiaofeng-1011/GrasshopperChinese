param([string]$LibrariesPath = (Join-Path $env:APPDATA 'Grasshopper\Libraries'))
$ErrorActionPreference = 'Stop'
if (Get-Process Rhino -ErrorAction SilentlyContinue) { throw 'Close Rhino before uninstalling.' }
$destination = Join-Path $LibrariesPath 'GrasshopperChinese'
foreach ($name in @('GrasshopperChinese.gha', 'Languages\zh-CN.json', 'Languages\tool-text.zh-CN.json', 'LICENSE-MultilingualGH.txt', 'README.md', 'THIRD-PARTY-NOTICES.md')) {
    $target = Join-Path $destination $name
    if (Test-Path -LiteralPath $target -PathType Leaf) { Remove-Item -LiteralPath $target }
}
Write-Host 'Plugin files removed. User-created files and settings were preserved.'
