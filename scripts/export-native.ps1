param([switch]$CoreOnly)
$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$system = 'C:\Program Files\Rhino 8\System'
$ghRoot = 'C:\Program Files\Rhino 8\Plug-ins\Grasshopper'
foreach ($name in @('Eto.dll','RhinoCommon.dll','Mono.Cecil.dll','Rhino.UI.dll','Yak.Core.dll')) {
    $null = [Reflection.Assembly]::LoadFrom((Join-Path $system $name))
}
$null = [Reflection.Assembly]::LoadFrom("$ghRoot\GH_IO.dll")
$gh = [Reflection.Assembly]::LoadFrom("$ghRoot\Grasshopper.dll")
$assemblies = @($gh)
$nativeFiles = @('CurveComponents.gha','FieldComponents.gha','MathComponents.gha','SurfaceComponents.gha','TriangulationComponents.gha','VectorComponents.gha','XformComponents.gha','IOComponents.gha')
if (!$CoreOnly) { foreach ($name in $nativeFiles) { $assemblies += [Reflection.Assembly]::LoadFrom("$ghRoot\Components\$name") } }
$rows = New-Object 'System.Collections.Generic.List[object]'
$failures = New-Object 'System.Collections.Generic.List[object]'
foreach ($assembly in $assemblies) {
    try { $types = $assembly.GetTypes() }
    catch [Reflection.ReflectionTypeLoadException] { $types = $_.Exception.Types | Where-Object { $_ } }
    foreach ($type in $types) {
        if ($type.IsAbstract -or !$type.IsPublic -or !$gh.GetType('Grasshopper.Kernel.IGH_DocumentObject').IsAssignableFrom($type) -or !$type.GetConstructor([Type[]]@())) { continue }
        try {
            $obj = [Activator]::CreateInstance($type)
            if (!$obj.Name -or !$obj.Category) { continue }
            $inputs = @(); $outputs = @()
            if ($obj -is [Grasshopper.Kernel.IGH_Component]) {
                $inputs = @($obj.Params.Input | ForEach-Object { @{name=$_.Name; nickname=$_.NickName; description=$_.Description} })
                $outputs = @($obj.Params.Output | ForEach-Object { @{name=$_.Name; nickname=$_.NickName; description=$_.Description} })
            }
            $rows.Add(@{guid=$obj.ComponentGuid.ToString(); name=$obj.Name; nickname=$obj.NickName; description=$obj.Description; category=$obj.Category; subcategory=$obj.SubCategory; inputs=$inputs; outputs=$outputs; type=$type.FullName; assembly=$assembly.GetName().Name})
        }
        catch { $failures.Add(@{type=$type.FullName; error=$_.Exception.GetBaseException().Message}) }
    }
    Write-Host ($assembly.GetName().Name + ': total ' + $rows.Count + ', failed ' + $failures.Count)
}
$output = Join-Path $root 'docs\native-catalog.json'
@{sdk=$gh.GetName().Version.ToString(); tools=@($rows.ToArray()); failures=@($failures.ToArray())} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $output -Encoding UTF8
Write-Host "Catalog: $output"
