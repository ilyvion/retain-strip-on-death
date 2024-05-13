$ErrorActionPreference = 'Stop'

$Configuration = 'Release'

$Target = "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\RetainStripOnDeath"

# build dlls
$env:RimWorldVersion = "1.4"
dotnet build --configuration $Configuration .vscode/mod.csproj
if ($LASTEXITCODE -gt 0) {
    throw "Build failed"
}
Start-Sleep -Seconds 0.5
$env:RimWorldVersion = "1.5"
dotnet build --configuration $Configuration .vscode/mod.csproj
if ($LASTEXITCODE -gt 0) {
    throw "Build failed"
}

# remove pdbs (for release)
if ($Configuration -eq "Release") {
    Remove-Item -Path .\1.5\Assemblies\RetainStripOnDeath.pdb -ErrorAction SilentlyContinue
    Remove-Item -Path .\1.4\Assemblies\RetainStripOnDeath.pdb -ErrorAction SilentlyContinue
}

# remove mod folder
Remove-Item -Path $Target -Recurse

# copy mod files
Copy-Item -Path 1.4 $Target\1.4 -Recurse
Copy-Item -Path 1.5 $Target\1.5 -Recurse

Copy-Item -Path Common $Target\Common -Recurse
Copy-Item -Path About $Target\About -Recurse -Exclude *.pdn

Copy-Item -Path CHANGELOG.md $Target
Copy-Item -Path LICENSE $Target
Copy-Item -Path LICENSE.Apache-2.0 $Target
Copy-Item -Path LICENSE.MIT $Target
Copy-Item -Path README.md $Target
