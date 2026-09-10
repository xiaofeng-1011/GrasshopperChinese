$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$testRoot = Join-Path $root ('build\install-test-' + [Guid]::NewGuid().ToString('N'))
$null = New-Item -ItemType Directory -Path $testRoot
$libraries = Join-Path $testRoot 'Libraries'
& "$root\installer\Install.ps1" -LibrariesPath $libraries
if (!(Test-Path "$libraries\GrasshopperChinese\GrasshopperChinese.gha")) { throw 'GHA was not installed' }
if (!(Test-Path "$libraries\GrasshopperChinese\Languages\zh-CN.json")) { throw 'Language pack missing' }
Set-Content -LiteralPath "$libraries\GrasshopperChinese\user-note.txt" -Value 'Preserve me'
& "$root\installer\Uninstall.ps1" -LibrariesPath $libraries
if (Test-Path "$libraries\GrasshopperChinese\GrasshopperChinese.gha") { throw 'GHA not removed' }
if (!(Test-Path "$libraries\GrasshopperChinese\user-note.txt")) { throw 'User file deleted' }
Write-Host 'PASS install package, include dictionary, uninstall only owned files'
