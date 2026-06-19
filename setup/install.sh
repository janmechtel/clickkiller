#!/bin/bash
# Install ClickKiller desktop integration via symlinks into the repo.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
APPLICATIONS_DIR="${HOME}/Applications"
AUTOSTART_DIR="${HOME}/.config/autostart"
NIRI_BINDS_FILE="${HOME}/.config/niri/dms/binds.kdl"

mkdir -p "$APPLICATIONS_DIR" "$AUTOSTART_DIR"

chmod +x "$REPO_ROOT/setup/run-clickkiller.sh" "$REPO_ROOT/setup/clickkiller-trigger.sh"

ln -sfn "$REPO_ROOT/setup/clickkiller-trigger.sh" "$APPLICATIONS_DIR/clickkiller-trigger.sh"
ln -sfn "$REPO_ROOT/setup/run-clickkiller.sh" "$APPLICATIONS_DIR/clickkiller-run.sh"
ln -sfn "$REPO_ROOT/setup/clickkiller.desktop" "$AUTOSTART_DIR/clickkiller.desktop"

echo "Symlinked:"
echo "  $APPLICATIONS_DIR/clickkiller-trigger.sh -> setup/clickkiller-trigger.sh"
echo "  $APPLICATIONS_DIR/clickkiller-run.sh -> setup/run-clickkiller.sh"
echo "  $AUTOSTART_DIR/clickkiller.desktop -> setup/clickkiller.desktop"

if [ -f "$NIRI_BINDS_FILE" ]; then
    if rg -q 'clickkiller-trigger\.sh' "$NIRI_BINDS_FILE"; then
        echo "Niri F1 bind already present in $NIRI_BINDS_FILE"
    else
        echo
        echo "Add the F1 bind manually from setup/niri-bind.snippet.kdl to:"
        echo "  $NIRI_BINDS_FILE"
    fi
else
    echo
    echo "Niri binds file not found at $NIRI_BINDS_FILE"
    echo "Add the F1 bind from setup/niri-bind.snippet.kdl to your compositor config."
fi

echo
echo "Build the app if needed:"
echo "  dotnet publish clickkiller.root.csproj -c Release -r linux-x64 --self-contained -o publish"
