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
PUBLISH_DIR="$SCRIPT_DIR/../publish"

echo ""
echo "Compiling with dotnet..."
dotnet publish -c Release --self-contained -r linux-x64 -o "$PUBLISH_DIR"

#echo ""
#echo "Downloading Velopack Releases"
#vpk download github --repoUrl https://github.com/janmechtel/clickkiller/ -o "$RELEASE_DIR"

echo ""
echo "Building Velopack Release v$BUILD_VERSION"
vpk pack -u Clickkiller -v $BUILD_VERSION -o "$RELEASE_DIR" -p "$PUBLISH_DIR" --mainExe clickkiller.Linux

echo ""
echo "Uploading Velopack Releases"
#vpk upload github --repoUrl https://github.com/janmechtel/clickkiller/ --publish --releaseName "ClickKiler $BUILD_VERSION" --tag v$BUILD_VERSION -o "$RELEASE_DIR" --token 
gsutil -m cp -n $RELEASE_DIR/*.nupkg gs://clickkiller/ # -n skip existing files
gsutil -m cp $RELEASE_DIR/*.json $RELEASE_DIR/RELEASES-* $RELEASE_DIR/*.AppImage $RELEASE_DIR/*.exe gs://clickkiller/


