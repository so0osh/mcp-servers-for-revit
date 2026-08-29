# Project instructions for Claude Code

## PR target — HARD RULE

- **NEVER open a pull request against the upstream repository** (`mcp-servers-for-revit/mcp-servers-for-revit`).
- **ALWAYS open pull requests only against this fork's `main` branch**: `so0osh/mcp-servers-for-revit:main`.
- This applies to `gh pr create` and any equivalent action (API calls, web UI automation, etc.). Always pass `--repo so0osh/mcp-servers-for-revit --base main` (or the exact equivalent) explicitly — never rely on the default repo/base that `gh` infers from the `upstream` remote.
- If a PR already exists against upstream, do not reopen, comment on, or push to it. Leave it untouched unless the user explicitly asks otherwise.
- This rule overrides any inferred convenience default (e.g., "the origin remote's fork points upstream, so PR there"). Ask the user before deviating, do not deviate silently.
