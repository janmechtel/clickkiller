#!/bin/bash

# Find the absolute path of the script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Check if version parameter is provided
if [ "$#" -ne 1 ]; then
    echo "Version number is required."
    echo "Usage: ./build.sh [version]"
    exit 1
fi

BUILD_VERSION="$1"
RELEASE_DIR="$SCRIPT_DIR/../releases"
PUBLISH_DIR="$SCRIPT_DIR/publish"
ICON=$SCRIPT_DIR/../clickkiller/Assets/clickkiller.ico
CHANNEL="win-stable"

echo ""
echo "Compiling with dotnet..."
dotnet publish -c Release --self-contained -r win-x64 -o "$PUBLISH_DIR"

echo ""
echo "Building Velopack Release v$BUILD_VERSION"
vpk "[win]" pack -u Clickkiller -v $BUILD_VERSION -o "$RELEASE_DIR" -p "$PUBLISH_DIR" -c "$CHANNEL" --mainExe clickkiller.Windows.exe --icon "$ICON"

echo ""
echo "Uploading Velopack Releases"
#vpk upload github --repoUrl https://github.com/janmechtel/clickkiller/ --publish --releaseName "ClickKiler Windows $BUILD_VERSION" --tag v$BUILD_VERSION -o "$RELEASE_DIR" --token 
# -n skip existing files
echo "gsutil -m cp -n $RELEASE_DIR/*.nupkg gs://clickkiller/"
echo "gsutil -m cp $RELEASE_DIR/*.json $RELEASE_DIR/RELEASES-* $RELEASE_DIR/*.exe $RELEASE_DIR/*.zip gs://clickkiller/"

