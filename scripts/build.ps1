param([string]$RhinoRoot = 'C:\Program Files\Rhino 8')
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$out = Join-Path $root 'build\GrasshopperChinese'
New-Item -ItemType Directory -Force -Path $out | Out-Null
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$refs = @('System.Drawing.dll', 'System.Windows.Forms.dll', 'System.Runtime.Serialization.dll', "$RhinoRoot\System\RhinoCommon.dll", "$RhinoRoot\Plug-ins\Grasshopper\Grasshopper.dll", "$RhinoRoot\Plug-ins\Grasshopper\GH_IO.dll")
$arguments = @('/nologo', '/utf8output', '/target:library', '/optimize+', '/platform:anycpu', "/out:$out\GrasshopperChinese.gha")
$arguments += $refs | ForEach-Object { '/r:' + $_ }
$arguments += @("$root\src\Core.cs", "$root\src\Plugin.cs", "$root\src\RibbonTranslator.cs")
& $compiler $arguments
if ($LASTEXITCODE -ne 0) { throw 'Plugin compilation failed' }
Copy-Item -LiteralPath "$root\Languages" -Destination $out -Recurse -Force
Copy-Item -LiteralPath "$root\upstream\LICENSE" -Destination "$out\LICENSE-MultilingualGH.txt" -Force
Write-Host "Built $out\GrasshopperChinese.gha"
