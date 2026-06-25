# Clickkiller + Kontext Redesign
_Design spec — 2026-06-25_

## Overview

Two focused tools with a clean boundary:

- **kontext** — event sink, HEAD tracker, AI enrichment layer. The protocol.
- **clickkiller** — unified launcher, todo manager, session viewer. The UI.

This spec covers the refined scope of the kontext daemon and the full redesign of clickkiller as the reference GUI that consumes it.

---

## 1. kontext Daemon (refined scope)

### Responsibilities

| Concern | Description |
|---|---|
| **Event sink** | Main writer for `thread.jsonl`. Receives events from niri, file opens, URL opens, pi sessions, goals, agent activity. Append-only. |
| **HEAD tracking** | Maintains a mutable pointer: current workspace, project, active goal. Updated on every relevant event. |
| **HEAD enrichment** | Runs AI inference on the thread. Produces `current.md` summaries. Derives the context object exposed to consumers. |
| **Socket API** | Single query endpoint: "give me current HEAD" → returns full context object. Also accepts incoming events from external tools. |

### HEAD context object (shape)

```json
{
  "project": "clickkiller",
  "project_path": "~/kontext/clickkiller",
  "workspace": 3,
  "active_goal": {
    "id": "afe13845",
    "title": "Redesign launcher UX",
    "note": ""
  },
  "summary": "Working on the launcher redesign. Key open question is modifier key UX.",
  "open_issues_count": 4,
  "active_agents": ["pi-session-abc123"],
  "ts": "2026-06-25T20:00:00Z"
}
```

### What kontext does NOT do

- Does not manage or own issues/todos
- Does not render any UI
- Does not enforce any schema on external tools' data
- Does not gate access to `thread.jsonl` — other tools may read it directly

### thread.jsonl events (existing + new)

All events: `{ id, parentId, type, ts, src, ws_num, goal_id, data }`

| Type | Written by | Meaning |
|---|---|---|
| `push` | daemon | New goal set |
| `prompt` | daemon (via pi watcher) | Pi session prompt |
| `response` | daemon (via pi watcher) | Pi session response |
| `ws_closed` | daemon | Workspace closed |
| `file_opened` | daemon | File opened in project |
| `url_opened` | daemon | URL opened in project context |
| `agent_started` | daemon / herdr | Agent session began |
| `issue_referenced` | any tool | External issue linked to thread node (e.g. JIRA-123) |

---

## 2. clickkiller (unified launcher + todo + session viewer)

### What it replaces

| Old component | Absorbed into |
|---|---|
| clickkiller (Avalonia/.NET) | clickkiller capture mode |
| kontextpanel (GTK4+WebKit panel) | clickkiller display panel |
| goal-router.ts (pi extension) | clickkiller goal capture flow |
| goal-launch.sh | clickkiller "Do Now" action |
| F3 workspace assignment | clickkiller "Update" verb |

### Tech stack

- **Shell**: GTK4 + gtk4-layer-shell (Wayland-native, proven from kontextpanel)
- **Renderer**: WebKit (embeds HTML UI, same as kontextpanel)
- **Server**: Language TBD (Python or Deno) serving HTML to WebKit locally
- **Platform**: Linux / Wayland / niri only

### Two surfaces

#### Surface A — Launcher popup

A small floating window. Opens fast, dismisses fast. One text input.

```
┌─────────────────────────────────┐
│  > ▊                            │
│                                 │
│  [current HEAD: Fix login bug]  │  ← shown in edit mode only
└─────────────────────────────────┘
```

**Keybinds to open:**

| Key | Opens as |
|---|---|
| `F1` | Empty — capture new thing |
| `Super+F2` | Pre-filled with current goal title — edit current node |

**Inside the launcher:**

| Key | Action |
|---|---|
| `F2` | Toggle between empty ↔ pre-filled with HEAD (switch modes) |
| `Enter` | **Do Later** → append to `issues.md` |
| `Ctrl+Enter` | **Do Now** → emit goal event to kontext, spawn new niri workspace |
| `Shift+Enter` | **Update current** → amend HEAD node (rename goal, reassign workspace) |
| `Escape` | Dismiss, discard |

**Text syntax:**
- `@project/path` mention → scopes the action to a specific project
- No mention → uses current HEAD project
- File paths, URLs — stored as-is in the issue body or thread event

#### Surface B — Display panel

A persistent side panel (right edge, same as kontextpanel). Toggle on/off.

```
┌──────────────────────┐
│ clickkiller          │
│ ──────────────────── │
│ ● Fix login redesign │  ← active goal
│                      │
│ AI Summary           │
│ Working on launcher  │
│ UX. Modifier keys    │
│ TBD.                 │
│                      │
│ Open Issues (4)      │
│ □ Fix mobile redirect│
│ □ Update README      │
│ □ Refactor auth      │
│                      │
│ Active Agents (1)    │
│ ◎ pi-session-abc123  │
│                      │
│ Recent thread        │
│ 14:23 prompt: ...    │
│ 14:21 goal: ...      │
└──────────────────────┘
```

**Keybind:** `Super+F1` (toggle, same muscle memory as current kontextpanel)

**Data sources (all read-only in this panel):**
- HEAD context object ← daemon socket
- `current.md` AI summary ← HEAD enrichment
- `issues.md` ← direct file read
- `thread.jsonl` ← direct file read (recent events)
- `agents.jsonl` ← direct file read

---

## 3. Issue storage

### Format

`issues.md` — markdown file, `##` headings as issues. Source of truth. Editable by hand at any time.

```markdown
## Fix login redirect on mobile
Chrome only. Noticed 2026-06-20.

## [done] Update onboarding README

## Refactor auth module
Blocks SSO work. Probably a week.

## +3 Tooltip performance on large lists
```

Conventions:
- `## Title` — open issue
- `## [done] Title` — closed issue
- `+N` prefix or line — vote count / urgency (+1 equivalent)
- Body text below heading = notes
- No YAML frontmatter — keep it plain and editable

### Locations

| File | Scope |
|---|---|
| `~/kontext/<project>/issues.md` | Per-project issues |
| `~/issues.md` | Global inbox — system friction, no-project context |

clickkiller writes to the project file when HEAD has a project. Falls back to `~/issues.md` when there's no current project context, or when the item is scoped to no project.

### No database

The markdown file is the database. A cache/DB layer can be added later if cross-project search or history replay is needed. YAGNI for now.

---

## 4. Boundary between kontext and clickkiller

```
Event flow (write path):
  niri event / file open / URL         → kontext daemon → thread.jsonl
  clickkiller "Do Now" (Ctrl+Enter)    → kontext daemon → thread.jsonl
  clickkiller "Update" (Shift+Enter)   → kontext daemon → thread.jsonl
  clickkiller "Do Later" (Enter)       → issues.md directly (no daemon)

Read path:
  clickkiller display panel            → reads HEAD from daemon socket
                                       → reads issues.md directly
                                       → reads thread.jsonl directly
                                       → reads current.md directly

Future tools (Jira plugin, Linear, etc.):
  → same pattern: query HEAD, emit events, own their state
```

The daemon is authoritative for the thread. clickkiller is authoritative for nothing — it's a view and a capture tool.

---

## 5. Migration plan (high level)

| Current state | Target state |
|---|---|
| clickkiller: Avalonia/.NET, SQLite issues.db | clickkiller: GTK4+WebKit, issues.md |
| kontextpanel: separate process, GTK4+WebKit panel | Merged into clickkiller display panel |
| goal-router.ts: pi extension, `/goal` command | Absorbed into clickkiller launcher (Ctrl+Enter flow) |
| goal-launch.sh: shell script, new workspace | Called by clickkiller "Do Now" action |
| F3: workspace assign keybind | Replaced by launcher Shift+Enter "Update" verb |
| SQLite issues: Id, Timestamp, Application, Notes, IsDone | Migrated to issues.md (one-time script) |
| Super+F1: kontextpanel toggle | Super+F1: clickkiller display panel toggle (same key) |

Migration of existing SQLite issues → `issues.md` is a one-time conversion script.

---

## 6. Open questions (deferred)

- **Tree placement on capture**: when creating a new issue/goal, where does it sit in the thread graph? (sub-issue vs sibling vs new root). Not designed yet — default to appending at current HEAD.
- **Backend language**: Python (extend kontext) or Deno (fresh start). Decision deferred — architecture is language-agnostic.
- **Display panel visual design**: no mockups yet.
- **`agents.jsonl` schema**: not defined. Depends on herdr/pi session management work.
- **issue_referenced event**: exact schema for linking an issue to a thread node TBD.
- **Cross-project issues**: what happens when `@other-project` is mentioned in capture — store in that project's issues.md or note the reference?
