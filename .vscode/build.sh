#!/usr/bin/env bash
set -e

CONFIGURATION="Release"
TARGET="$HOME/.var/app/com.valvesoftware.Steam/.local/share/Steam/steamapps/common/RimWorld/Mods/RetainStripOnDeath"

mkdir -p .savedatafolder/1.6
mkdir -p .savedatafolder/1.5
mkdir -p .savedatafolder/1.4

# build dlls
export RimWorldVersion="1.4"
dotnet build --configuration "$CONFIGURATION" .vscode/mod.csproj
sleep 1
export RimWorldVersion="1.5"
dotnet build --configuration "$CONFIGURATION" .vscode/mod.csproj
sleep 1
export RimWorldVersion="1.6"
dotnet build --configuration "$CONFIGURATION" .vscode/mod.csproj

# remove pdbs (for release)
if [ "$CONFIGURATION" = "Release" ]; then
    rm -f ./1.4/Assemblies/RetainStripOnDeath.pdb
    rm -f ./1.5/Assemblies/RetainStripOnDeath.pdb
    rm -f ./1.6/Assemblies/RetainStripOnDeath.pdb
fi

# remove mod folder
rm -rf "$TARGET"

# copy mod files
mkdir -p "$TARGET"
cp -r 1.4 "$TARGET/1.4"
cp -r 1.5 "$TARGET/1.5"
cp -r 1.6 "$TARGET/1.6"
cp -r Common "$TARGET/Common"
rsync -av --exclude='*.pdn' About/ "$TARGET/About"
cp CHANGELOG.md "$TARGET"
cp LICENSE "$TARGET"
cp LICENSE.Apache-2.0 "$TARGET"
cp LICENSE.MIT "$TARGET"
cp README.md "$TARGET"
