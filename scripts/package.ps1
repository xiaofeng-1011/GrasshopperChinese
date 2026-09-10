$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
& "$PSScriptRoot\test.ps1"
& "$PSScriptRoot\test-ribbon.ps1"
& "$PSScriptRoot\build.ps1"
& "$PSScriptRoot\test-tool-labels.ps1"
$payload = Join-Path $root 'build\GrasshopperChinese'
Copy-Item -LiteralPath "$root\README.md", "$root\THIRD-PARTY-NOTICES.md" -Destination $payload -Force
$stage = Join-Path $root ('build\package-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $stage | Out-Null
Copy-Item -LiteralPath $payload -Destination $stage -Recurse
Get-ChildItem -LiteralPath "$root\installer" -File | Copy-Item -Destination $stage
$dist = Join-Path $root 'dist'
New-Item -ItemType Directory -Force -Path $dist | Out-Null
$zip = Join-Path $dist 'GrasshopperChinese-0.3.0-preview-win.zip'
Compress-Archive -Path "$stage\*" -DestinationPath $zip -Force
$hash = Get-FileHash -LiteralPath $zip -Algorithm SHA256
Set-Content -LiteralPath "$zip.sha256" -Value ($hash.Hash.ToLowerInvariant() + '  ' + (Split-Path $zip -Leaf)) -Encoding ASCII
Write-Host "Packaged $zip"
