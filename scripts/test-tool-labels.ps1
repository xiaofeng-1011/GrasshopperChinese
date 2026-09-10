$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$sdk = 'C:\Program Files\Rhino 8\Plug-ins\Grasshopper\Grasshopper.dll'
$plugin = "$root\build\GrasshopperChinese\GrasshopperChinese.gha"
& $compiler /nologo /utf8output /target:library "/out:$root\build\ToolLabelTests.dll" "/r:$sdk" "/r:$plugin" '/r:C:\Program Files\Rhino 8\Plug-ins\Grasshopper\GH_IO.dll' /r:System.Drawing.dll /r:System.Windows.Forms.dll "$root\tests\ToolLabelTests.cs"
if ($LASTEXITCODE -ne 0) { throw 'Tool label test compilation failed' }
foreach ($name in @('Eto.dll','RhinoCommon.dll')) { $null=[Reflection.Assembly]::LoadFrom("C:\Program Files\Rhino 8\System\$name") }
$null=[Reflection.Assembly]::LoadFrom('C:\Program Files\Rhino 8\Plug-ins\Grasshopper\GH_IO.dll')
$null=[Reflection.Assembly]::LoadFrom($sdk)
$null=[Reflection.Assembly]::LoadFrom($plugin)
$assembly=[Reflection.Assembly]::LoadFrom("$root\build\ToolLabelTests.dll")
$result=$assembly.GetType('ToolLabelTests').GetMethod('Main',[Reflection.BindingFlags]'NonPublic,Static').Invoke($null,@())
if ($result -ne 0) { throw 'Tool label tests failed' }
