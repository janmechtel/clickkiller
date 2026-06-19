# ClickKiller local setup (Linux / niri)

These files live in the repo and are symlinked into your home directory by
`setup/install.sh`. That keeps desktop integration versioned here instead of
copied into `~/Applications` or `~/.config`.

## Quick start

```bash
# 1. Build the app
dotnet publish clickkiller.root.csproj -c Release -r linux-x64 --self-contained -o publish

# 2. Install symlinks for autostart + F1 trigger
./setup/install.sh
```

Press **F1** to toggle ClickKiller. The app also starts on login via autostart.

## What gets symlinked

| Repo file | Symlink target | Purpose |
|---|---|---|
| `setup/clickkiller-trigger.sh` | `~/Applications/clickkiller-trigger.sh` | F1 hotkey: toggle via Unix socket |
| `setup/run-clickkiller.sh` | `~/Applications/clickkiller-run.sh` | Autostart / direct launch |
| `setup/clickkiller.desktop` | `~/.config/autostart/clickkiller.desktop` | Start on login |

Scripts resolve the repo root from their own location, so they keep working as
long as the symlinks point back here.

## Niri hotkey (manual, outside repo)

Niri config is not symlinked from this repo. Add this line inside the
`binds { ... }` block in `~/.config/niri/dms/binds.kdl`:

```kdl
F1 hotkey-overlay-title="Clickkiller Toggle" { spawn "/home/jan/Applications/clickkiller-trigger.sh"; }
```

See `setup/niri-bind.snippet.kdl` for the same snippet.

Why not symlink the whole niri config? The niri config is shared with other
desktop settings (DMS, OBS, kontext, etc.). Only the one bind line references
ClickKiller.

## How F1 works

1. Niri runs `~/Applications/clickkiller-trigger.sh` (symlink into this repo)
2. If ClickKiller is running, the script sends `TriggerReport` to
   `/tmp/CoreFxPipe_ClickKillerPipe`
3. The app toggles its window open/closed
4. If the app is not running (or the socket is stale after a crash), the script
   starts `publish/clickkiller.root`

Global Alt+F1 inside the app (SharpHook) does not work on Wayland; use the niri
bind instead.

## Logs and data

- Logs: `~/.config/clickkiller/clickkiller*.txt`
- Database: `~/.config/clickkiller/issues.db`
- Lock / IPC socket: `~/.config/clickkiller/.lock`, `/tmp/CoreFxPipe_ClickKillerPipe`

## Rebuild after code changes

```bash
dotnet publish clickkiller.root.csproj -c Release -r linux-x64 --self-contained -o publish
pkill -f clickkiller.root || true
./setup/run-clickkiller.sh &
```

No need to rerun `install.sh` unless symlinks were removed.

## Optional: AppImage

The published AppImage in `~/Applications/Clickkiller.AppImage` is optional.
This repo setup uses the local `publish/clickkiller.root` build instead so F1
fixes take effect immediately after rebuild.
