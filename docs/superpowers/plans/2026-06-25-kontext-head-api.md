# Kontext HEAD API Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extend the kontext daemon's socket API to expose a rich HEAD context object — current workspace, project, active goal, AI summary, open issue count, and active agents — so any consumer (clickkiller, status bars, CLI) can query the full current context in one call.

**Architecture:** A new `get_head` socket command is added to `socket_server.py`. The HEAD object is assembled by `state.py` from existing in-memory state plus a new `head.py` module that reads `current.md` and `issues.md`. The daemon's existing `handle_switch()` flow continues to maintain state; `get_head` is a pure read.

**Tech Stack:** Python 3.11+, existing kontext codebase at `~/kontext/kontext/src/kontext/`

## Global Constraints

- All code lives in `~/kontext/kontext/src/kontext/`
- Tests live in `~/kontext/kontext/tests/`
- Run tests with: `pytest tests/ -v` from `~/kontext/kontext/`
- Socket protocol: send JSON line, receive JSON line (existing pattern)
- HEAD object must be JSON-serialisable
- No new dependencies — use only stdlib + what's already in `pyproject.toml`
- Python type hints required on all new public functions

---

### Task 1: Define the HEAD data model

**Files:**
- Create: `src/kontext/head.py`
- Create: `tests/test_head.py`

**Interfaces:**
- Produces:
  - `HeadContext` dataclass with fields: `project`, `project_path`, `workspace`, `active_goal`, `summary`, `open_issues_count`, `active_agents`, `ts`
  - `head_to_dict(head: HeadContext) -> dict` — JSON-serialisable dict
  - `empty_head() -> HeadContext` — safe default when no context is active

- [ ] **Step 1: Write failing tests**

Create `tests/test_head.py`:

```python
from kontext.head import HeadContext, head_to_dict, empty_head
import datetime

def test_empty_head_is_valid():
    h = empty_head()
    assert h.project is None
    assert h.workspace is None
    assert h.active_goal is None
    assert h.open_issues_count == 0
    assert h.active_agents == []

def test_head_to_dict_is_json_serialisable():
    import json
    h = HeadContext(
        project="clickkiller",
        project_path="/home/jan/kontext/clickkiller",
        workspace=3,
        active_goal={"id": "abc123", "title": "Fix login", "note": ""},
        summary="Working on login fix.",
        open_issues_count=4,
        active_agents=["pi-session-abc"],
        ts=datetime.datetime(2026, 6, 25, 20, 0, 0, tzinfo=datetime.timezone.utc),
    )
    d = head_to_dict(h)
    json.dumps(d)  # must not raise
    assert d["project"] == "clickkiller"
    assert d["active_goal"]["title"] == "Fix login"
    assert d["ts"] == "2026-06-25T20:00:00+00:00"

def test_head_to_dict_none_fields_are_null():
    h = empty_head()
    d = head_to_dict(h)
    assert d["project"] is None
    assert d["active_goal"] is None
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
cd ~/kontext/kontext
pytest tests/test_head.py -v
```

Expected: `ModuleNotFoundError: No module named 'kontext.head'`

- [ ] **Step 3: Implement `src/kontext/head.py`**

```python
"""HEAD context model — the current active context exposed to consumers."""
from __future__ import annotations

import datetime
from dataclasses import dataclass, field
from typing import Any


@dataclass
class HeadContext:
    project: str | None = None
    project_path: str | None = None
    workspace: int | None = None
    active_goal: dict[str, Any] | None = None  # {id, title, note}
    summary: str = ""
    open_issues_count: int = 0
    active_agents: list[str] = field(default_factory=list)
    ts: datetime.datetime = field(
        default_factory=lambda: datetime.datetime.now(datetime.timezone.utc)
    )


def empty_head() -> HeadContext:
    """Return a safe default HEAD when no context is active."""
    return HeadContext()


def head_to_dict(head: HeadContext) -> dict[str, Any]:
    """Convert HeadContext to a JSON-serialisable dict."""
    return {
        "project": head.project,
        "project_path": head.project_path,
        "workspace": head.workspace,
        "active_goal": head.active_goal,
        "summary": head.summary,
        "open_issues_count": head.open_issues_count,
        "active_agents": list(head.active_agents),
        "ts": head.ts.isoformat(),
    }
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
cd ~/kontext/kontext
pytest tests/test_head.py -v
```

Expected: 3 tests PASS

- [ ] **Step 5: Commit**

```bash
git add src/kontext/head.py tests/test_head.py
git commit -m "feat(head): add HeadContext dataclass and serialisation"
```

---

### Task 2: HEAD reader — assembles HeadContext from disk + state

**Files:**
- Modify: `src/kontext/head.py` (add `read_head()`)
- Create: `tests/test_head_reader.py`

**Interfaces:**
- Consumes:
  - `HeadContext`, `head_to_dict`, `empty_head` from Task 1
  - `state.LAST_PROJECT` (str | None), `state.LAST_WS_NUM` (int | None) from `state.py`
  - `state.CURRENT_LOG` (ThreadLog | None) from `state.py`
- Produces:
  - `read_head(project_path: str | None, ws_num: int | None, goal_log: Any | None) -> HeadContext`

- [ ] **Step 1: Inspect existing state module**

```bash
grep -n "LAST_PROJECT\|LAST_WS_NUM\|CURRENT_LOG\|current_log" \
  ~/kontext/kontext/src/kontext/state.py | head -20
```

Note the exact variable names — use them verbatim in the implementation.

- [ ] **Step 2: Write failing tests**

Create `tests/test_head_reader.py`:

```python
import os
import tempfile
import textwrap
from pathlib import Path
from kontext.head import read_head

def _make_project(tmp: Path, summary: str = "", issues: str = "") -> Path:
    kontext_dir = tmp / ".kontext"
    kontext_dir.mkdir()
    if summary:
        (kontext_dir / "current.md").write_text(summary)
    if issues:
        (tmp / "issues.md").write_text(issues)
    return tmp

def test_read_head_no_project():
    h = read_head(project_path=None, ws_num=None, goal_log=None)
    assert h.project is None
    assert h.open_issues_count == 0

def test_read_head_reads_summary(tmp_path):
    p = _make_project(tmp_path, summary="Working on login fix.")
    h = read_head(project_path=str(p), ws_num=2, goal_log=None)
    assert h.summary == "Working on login fix."
    assert h.workspace == 2

def test_read_head_counts_open_issues(tmp_path):
    issues = textwrap.dedent("""\
        ## Fix login redirect
        Notes here.

        ## [done] Update README

        ## Refactor auth
    """)
    p = _make_project(tmp_path, issues=issues)
    h = read_head(project_path=str(p), ws_num=1, goal_log=None)
    assert h.open_issues_count == 2  # [done] excluded

def test_read_head_missing_files_ok(tmp_path):
    p = _make_project(tmp_path)  # no summary, no issues.md
    h = read_head(project_path=str(p), ws_num=1, goal_log=None)
    assert h.summary == ""
    assert h.open_issues_count == 0
```

- [ ] **Step 3: Run tests to verify they fail**

```bash
cd ~/kontext/kontext
pytest tests/test_head_reader.py -v
```

Expected: `ImportError` — `read_head` not yet defined

- [ ] **Step 4: Implement `read_head` in `src/kontext/head.py`**

Append to the existing `head.py`:

```python
import re
from pathlib import Path


def _count_open_issues(project_path: str) -> int:
    """Count ## headings in issues.md that are not marked [done]."""
    issues_file = Path(project_path) / "issues.md"
    if not issues_file.exists():
        return 0
    count = 0
    for line in issues_file.read_text(encoding="utf-8").splitlines():
        if re.match(r"^##\s+(?!\[done\])", line, re.IGNORECASE):
            count += 1
    return count


def _read_summary(project_path: str) -> str:
    """Read AI summary from .kontext/current.md if it exists."""
    summary_file = Path(project_path) / ".kontext" / "current.md"
    if not summary_file.exists():
        return ""
    return summary_file.read_text(encoding="utf-8").strip()


def read_head(
    project_path: str | None,
    ws_num: int | None,
    goal_log: Any | None,
) -> HeadContext:
    """Assemble a HeadContext from current daemon state + disk reads."""
    if project_path is None:
        return empty_head()

    project_name = Path(project_path).name
    summary = _read_summary(project_path)
    open_issues = _count_open_issues(project_path)

    active_goal: dict[str, Any] | None = None
    if goal_log is not None:
        # ThreadLog exposes _derive() which returns current goal info
        try:
            derived = goal_log._derive()
            if derived.get("goal_id"):
                active_goal = {
                    "id": derived["goal_id"],
                    "title": derived.get("goal_title", ""),
                    "note": derived.get("goal_note", ""),
                }
        except Exception:
            pass  # log is empty or malformed — skip gracefully

    return HeadContext(
        project=project_name,
        project_path=project_path,
        workspace=ws_num,
        active_goal=active_goal,
        summary=summary,
        open_issues_count=open_issues,
        active_agents=[],  # populated by socket_server from live state
        ts=datetime.datetime.now(datetime.timezone.utc),
    )
```

- [ ] **Step 5: Run tests to verify they pass**

```bash
cd ~/kontext/kontext
pytest tests/test_head_reader.py -v
```

Expected: 4 tests PASS

- [ ] **Step 6: Commit**

```bash
git add src/kontext/head.py tests/test_head_reader.py
git commit -m "feat(head): add read_head() assembles context from disk + state"
```

---

### Task 3: Wire `get_head` into the socket server

**Files:**
- Modify: `src/kontext/socket_server.py`
- Create: `tests/test_socket_get_head.py`

**Interfaces:**
- Consumes: `read_head`, `head_to_dict` from `src/kontext/head.py`
- Consumes: `state.LAST_PROJECT`, `state.LAST_WS_NUM`, current log from existing state
- Produces: socket command `{"type": "get_head"}` → returns `{"ok": true, "head": {...}}`

- [ ] **Step 1: Inspect socket_server.py to understand dispatch pattern**

```bash
grep -n "def handle\|type.*==\|\"type\"" \
  ~/kontext/kontext/src/kontext/socket_server.py | head -30
```

Note the exact pattern used to dispatch on event type — replicate it exactly.

- [ ] **Step 2: Write a failing integration test**

Create `tests/test_socket_get_head.py`:

```python
"""Integration test — starts socket server, sends get_head, checks response."""
import json
import socket
import threading
import time
import pytest
from kontext import state
from kontext.socket_server import SocketServer


@pytest.fixture
def server(tmp_path):
    sock_path = str(tmp_path / "test.sock")
    srv = SocketServer(sock_path)
    t = threading.Thread(target=srv.serve_forever, daemon=True)
    t.start()
    time.sleep(0.05)  # let it bind
    yield sock_path
    srv.shutdown()


def _query(sock_path: str, payload: dict) -> dict:
    with socket.socket(socket.AF_UNIX, socket.SOCK_STREAM) as s:
        s.connect(sock_path)
        s.sendall((json.dumps(payload) + "\n").encode())
        return json.loads(s.recv(4096).decode())


def test_get_head_returns_ok(server):
    state.LAST_PROJECT = None
    state.LAST_WS_NUM = None
    resp = _query(server, {"type": "get_head"})
    assert resp["ok"] is True
    assert "head" in resp
    assert resp["head"]["project"] is None


def test_get_head_with_project(server, tmp_path):
    (tmp_path / ".kontext").mkdir()
    (tmp_path / ".kontext" / "current.md").write_text("Active work.")
    state.LAST_PROJECT = str(tmp_path)
    state.LAST_WS_NUM = 2
    resp = _query(server, {"type": "get_head"})
    assert resp["ok"] is True
    assert resp["head"]["project"] == tmp_path.name
    assert resp["head"]["workspace"] == 2
    assert resp["head"]["summary"] == "Active work."
```

- [ ] **Step 3: Run tests to verify they fail**

```bash
cd ~/kontext/kontext
pytest tests/test_socket_get_head.py -v
```

Expected: FAIL — `get_head` command not handled

- [ ] **Step 4: Add `get_head` handler to `socket_server.py`**

In `socket_server.py`, import at top:

```python
from kontext.head import read_head, head_to_dict
from kontext import state
```

Inside the dispatch block (replicate existing `if event_type == "..."` pattern):

```python
if event_type == "get_head":
    head = read_head(
        project_path=state.LAST_PROJECT,
        ws_num=state.LAST_WS_NUM,
        goal_log=state.CURRENT_LOG[0] if state.CURRENT_LOG else None,
    )
    return {"ok": True, "head": head_to_dict(head)}
```

(Adjust `state.CURRENT_LOG` access to match whatever pattern the existing code uses — check Step 1 output.)

- [ ] **Step 5: Run tests to verify they pass**

```bash
cd ~/kontext/kontext
pytest tests/test_socket_get_head.py -v
```

Expected: 2 tests PASS

- [ ] **Step 6: Run full test suite to check for regressions**

```bash
cd ~/kontext/kontext
pytest tests/ -v
```

Expected: all previously passing tests still pass

- [ ] **Step 7: Commit**

```bash
git add src/kontext/socket_server.py tests/test_socket_get_head.py
git commit -m "feat(socket): add get_head command to socket API"
```

---

### Task 4: CLI command `kontext head`

**Files:**
- Modify: `src/kontext/cli.py`
- (No new test file — covered by manual smoke test)

**Interfaces:**
- Consumes: existing `_emit()` helper in `cli.py` (sends JSON to socket, returns response)
- Produces: `kontext head` — prints HEAD as formatted JSON to stdout

- [ ] **Step 1: Find the _emit pattern in cli.py**

```bash
grep -n "_emit\|def _emit\|socket" ~/kontext/kontext/src/kontext/cli.py | head -20
```

- [ ] **Step 2: Add `head` subcommand to cli.py**

Find where subcommands are registered (look for `@app.command()` or `subparsers.add_parser`). Add:

```python
@app.command()
def head():
    """Print the current HEAD context as JSON."""
    import json
    result = _emit({"type": "get_head"})
    if result.get("ok"):
        print(json.dumps(result["head"], indent=2))
    else:
        print(f"Error: {result}", file=sys.stderr)
        raise SystemExit(1)
```

(Adjust decorator/pattern to match existing CLI framework — could be `typer`, `argparse`, or `click`. Check the imports at top of cli.py.)

- [ ] **Step 3: Smoke test manually**

```bash
cd ~/kontext/kontext
kontext head
```

Expected output (something like):
```json
{
  "project": "kontext",
  "project_path": "/home/jan/kontext/kontext",
  "workspace": 1,
  "active_goal": null,
  "summary": "",
  "open_issues_count": 0,
  "active_agents": [],
  "ts": "2026-06-25T20:00:00+00:00"
}
```

- [ ] **Step 4: Commit**

```bash
git add src/kontext/cli.py
git commit -m "feat(cli): add 'kontext head' command"
```
