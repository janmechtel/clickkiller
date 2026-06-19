#!/bin/bash
# Toggle ClickKiller via its Unix domain socket, or start the app.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")/.." && pwd)"
PIPE=/tmp/CoreFxPipe_ClickKillerPipe
APP="$REPO_ROOT/publish/clickkiller.root"

if [ -S "$PIPE" ]; then
    if printf 'TriggerReport\n' | ncat --send-only -U "$PIPE" 2>/dev/null; then
        exit 0
    fi
    # Stale socket left behind after a crash.
    rm -f "$PIPE"
fi

if [ ! -x "$APP" ]; then
    echo "ClickKiller binary not found at $APP" >&2
    echo "Build it first: dotnet publish clickkiller.root.csproj -c Release -r linux-x64 --self-contained -o publish" >&2
    exit 1
fi

exec "$APP" >/dev/null 2>&1 &
