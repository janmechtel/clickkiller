#!/bin/bash
# Launch ClickKiller from the repo publish output.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")/.." && pwd)"
APP="$REPO_ROOT/publish/clickkiller.root"

if [ ! -x "$APP" ]; then
    echo "ClickKiller binary not found at $APP" >&2
    echo "Build it first: dotnet publish clickkiller.root.csproj -c Release -r linux-x64 --self-contained -o publish" >&2
    exit 1
fi

exec "$APP" "$@"
