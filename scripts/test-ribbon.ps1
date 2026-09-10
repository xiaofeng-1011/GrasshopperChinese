$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$out = Join-Path $root 'build'
$sdk = 'C:\Program Files\Rhino 8\Plug-ins\Grasshopper\Grasshopper.dll'
& $compiler /nologo /utf8output /target:library "/out:$out\RibbonTests.dll" "/r:$sdk" /r:System.Drawing.dll /r:System.Windows.Forms.dll "$root\src\RibbonTranslator.cs" "$root\tests\RibbonTests.cs"
if ($LASTEXITCODE -ne 0) { throw 'Ribbon test compilation failed' }
$null = [Reflection.Assembly]::LoadFrom('C:\Program Files\Rhino 8\System\RhinoCommon.dll')
$null = [Reflection.Assembly]::LoadFrom($sdk)
$assembly = [Reflection.Assembly]::LoadFrom("$out\RibbonTests.dll")
$method = $assembly.GetType('RibbonTests').GetMethod('Main', [Reflection.BindingFlags]'Static,NonPublic')
if ($method.Invoke($null, @()) -ne 0) { throw 'Ribbon tests failed' }
