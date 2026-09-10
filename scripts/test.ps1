$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$out = Join-Path $root 'build'
New-Item -ItemType Directory -Force -Path $out | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
& $compiler /nologo /utf8output /target:exe "/out:$out\CoreTests.exe" /r:System.Runtime.Serialization.dll "$root\src\Core.cs" "$root\tests\CoreTests.cs"
if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed' }
& "$out/CoreTests.exe"
if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }
