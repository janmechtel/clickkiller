# Repo Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move clickkiller into the kontext monorepo as `src/clickkiller/`, archive the old repo, and copy the spec + this plan to their canonical location.

**Architecture:** The kontext repo at `~/kontext/kontext/` becomes the monorepo. The existing C# clickkiller source is preserved in `src/clickkiller/legacy-csharp/` for reference during migration. New clickkiller code will live in `src/clickkiller/`. The old `~/kontext/clickkiller` repo is archived on GitHub.

**Tech Stack:** git, GitHub CLI (`gh`)

## Global Constraints

- Platform: Linux (all commands are bash)
- Kontext repo: `~/kontext/kontext/`
- Old clickkiller repo: `~/kontext/clickkiller/`
- Target path for legacy code: `~/kontext/kontext/src/clickkiller/legacy-csharp/`
- Target path for new clickkiller: `~/kontext/kontext/src/clickkiller/`
- GitHub repo to archive: `janmechtel/clickkiller`

---

### Task 1: Copy legacy clickkiller source into kontext monorepo

**Files:**
- Create: `src/clickkiller/legacy-csharp/` (directory with copied C# source)
- Create: `src/clickkiller/README.md`

**Interfaces:**
- Produces: `src/clickkiller/legacy-csharp/` available in kontext repo for reference

- [ ] **Step 1: Copy the C# source (excluding git, build artifacts, nuget cache)**

```bash
cd ~/kontext/kontext
mkdir -p src/clickkiller/legacy-csharp

rsync -av \
  --exclude='.git' \
  --exclude='bin/' \
  --exclude='obj/' \
  --exclude='.vs/' \
  --exclude='*.user' \
  --exclude='publish/' \
  ~/kontext/clickkiller/ \
  src/clickkiller/legacy-csharp/
```

- [ ] **Step 2: Create a README for the new clickkiller directory**

Create `src/clickkiller/README.md`:

```markdown
# clickkiller

Unified launcher, todo manager, and session viewer for the kontext ecosystem.

## Structure

- `legacy-csharp/` — original Avalonia/.NET implementation (reference only, not built)
- (new implementation files will appear here)

## Spec

See `../../docs/superpowers/specs/2026-06-25-clickkiller-kontext-redesign.md`
```

- [ ] **Step 3: Verify the copy looks right**

```bash
ls ~/kontext/kontext/src/clickkiller/
ls ~/kontext/kontext/src/clickkiller/legacy-csharp/
```

Expected: `legacy-csharp/` contains `clickkiller/`, `clickkiller.sln`, `setup/`, `docs/`, etc.

- [ ] **Step 4: Commit**

```bash
cd ~/kontext/kontext
git add src/clickkiller/
git commit -m "chore: import clickkiller legacy C# source into monorepo"
```

---

### Task 2: Copy specs and plans to canonical kontext location

**Files:**
- Modify: `docs/superpowers/specs/` (already has the spec from brainstorming)
- Create: `docs/superpowers/plans/2026-06-25-repo-migration.md` (this file)
- Create: `docs/superpowers/plans/2026-06-25-kontext-head-api.md`

**Interfaces:**
- Produces: all planning docs live in the kontext repo

- [ ] **Step 1: Copy this plan to the kontext repo**

```bash
cp ~/kontext/clickkiller/docs/superpowers/plans/2026-06-25-repo-migration.md \
   ~/kontext/kontext/docs/superpowers/plans/

cp ~/kontext/clickkiller/docs/superpowers/plans/2026-06-25-kontext-head-api.md \
   ~/kontext/kontext/docs/superpowers/plans/ 2>/dev/null || true
```

- [ ] **Step 2: Commit**

```bash
cd ~/kontext/kontext
git add docs/superpowers/plans/
git commit -m "docs: add migration and HEAD API implementation plans"
```

---

### Task 3: Archive the old clickkiller repo on GitHub

**Files:** No local files changed.

**Interfaces:**
- Produces: `janmechtel/clickkiller` GitHub repo is archived (read-only)

- [ ] **Step 1: Verify gh CLI is authenticated**

```bash
gh auth status
```

Expected: `Logged in to github.com as janmechtel`

- [ ] **Step 2: Archive the repo**

```bash
gh repo archive janmechtel/clickkiller --yes
```

Expected output: `✓ Archived repository janmechtel/clickkiller`

- [ ] **Step 3: Verify it's archived**

```bash
gh repo view janmechtel/clickkiller --json isArchived --jq '.isArchived'
```

Expected: `true`

- [ ] **Step 4: Add archive notice to old repo README (optional but clear)**

```bash
cd ~/kontext/clickkiller
cat > ARCHIVED.md << 'EOF'
# ⚠️ This repository is archived

clickkiller has been merged into the [kontext monorepo](https://github.com/janmechtel/kontext) as `src/clickkiller/`.

New development happens there. This repo is preserved for history only.
EOF

git add ARCHIVED.md
git commit -m "chore: add archive notice"
git push
```

---

### Task 4: Update niri config comment (documentation only)

This task updates the niri keybind comment to note the migration is in progress.

**Files:**
- Modify: `~/.config/niri/dms/binds.kdl` (comment only, no functional change)

- [ ] **Step 1: Find the clickkiller F1 bind**

```bash
grep -n "clickkiller" ~/.config/niri/dms/binds.kdl
```

- [ ] **Step 2: Add a TODO comment above it**

Edit the file to add above the F1 bind:
```kdl
// TODO: clickkiller being rewritten — see ~/kontext/kontext/src/clickkiller/
```

- [ ] **Step 3: Verify niri config is still valid**

```bash
niri validate --config ~/.config/niri/config.kdl 2>&1 | head -5
```

Expected: no errors

- [ ] **Step 4: Commit the niri config change**

```bash
cd ~/kontext/linux  # or wherever niri config is tracked
git add -p
git commit -m "chore: note clickkiller rewrite in progress"
```

(Skip this step if niri config is not under version control)
