#!/bin/bash
set -euo pipefail

# Find the absolute path of the script
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Prefer user-local dotnet installation when available.
if [ -x "$HOME/.dotnet/dotnet" ]; then
    export DOTNET_ROOT="$HOME/.dotnet"
    export PATH="$DOTNET_ROOT:$HOME/.dotnet/tools:$PATH"
fi

# Check if version parameter is provided
if [ "$#" -ne 1 ]; then
    echo "Version number is required."
    echo "Usage: ./build-linux.sh [version]"
    exit 1
fi

if ! command -v dotnet >/dev/null; then
    echo "dotnet SDK is required but was not found on PATH."
    exit 1
fi

if ! command -v vpk >/dev/null; then
    echo "vpk CLI is required but was not found on PATH (install with: dotnet tool install -g vpk)."
    exit 1
fi

BUILD_VERSION="$1"
RELEASE_DIR="$SCRIPT_DIR/../releases"
PUBLISH_DIR="$SCRIPT_DIR/../publish"

echo ""
echo "Compiling with dotnet..."
dotnet publish "$SCRIPT_DIR/clickkiller.Linux.csproj" -c Release --self-contained -r linux-x64 -o "$PUBLISH_DIR"

#echo ""
#echo "Downloading Velopack Releases"
vpk download github --repoUrl https://github.com/janmechtel/clickkiller/ -o "$RELEASE_DIR"

echo ""
echo "Building Velopack Release v$BUILD_VERSION"
vpk pack -u Clickkiller -v $BUILD_VERSION -o "$RELEASE_DIR" -p "$PUBLISH_DIR" --mainExe clickkiller.Linux

echo ""
echo "Uploading Velopack Releases to GitHub"
if [ -z "${GITHUB_TOKEN:-}" ]; then
    if command -v gh >/dev/null; then
        if GH_TOKEN_VALUE="$(gh auth token)"; then
            GITHUB_TOKEN="$GH_TOKEN_VALUE"
        fi
    fi
fi
if [ -z "${GITHUB_TOKEN:-}" ]; then
    echo "GITHUB_TOKEN is required. Set it explicitly or run 'gh auth login'."
    exit 1
fi
vpk upload github --repoUrl https://github.com/janmechtel/clickkiller/ --publish --releaseName "Clickkiller $BUILD_VERSION" --tag v$BUILD_VERSION -o "$RELEASE_DIR" --token "$GITHUB_TOKEN"


