# GitHub Workflow — Solo Dev

## Daily workflow

1. Push to `main` directly for small changes
2. Use a feature branch + PR only for bigger changes you want CI to validate first
3. Release via one-click workflow — everything else is automated

## Release process

### Option A: One-click release (recommended)

1. Go to **Actions** tab > **Release** workflow > **Run workflow**
2. Pick bump type: `patch` / `minor` / `major`
3. Done. The workflow:
   - Reads current version from `.csproj`
   - Bumps version in `KwtSMS.csproj` + `KwtSMS.Cli.csproj`
   - Runs tests
   - Commits "Release vX.Y.Z"
   - Tags and pushes
   - Triggers `publish.yml` which publishes to NuGet + creates GitHub Release

### Option B: Manual release

```bash
# 1. Bump version in src/KwtSMS/KwtSMS.csproj and tools/KwtSMS.Cli/KwtSMS.Cli.csproj
# 2. Commit
git add src/KwtSMS/KwtSMS.csproj tools/KwtSMS.Cli/KwtSMS.Cli.csproj
git commit -m "Release v0.4.0"

# 3. Tag and push
git tag v0.4.0
git push && git push --tags
```

## Automation overview

| Workflow | Trigger | What it does |
|----------|---------|-------------|
| CI | Push/PR to main | Tests on .NET 6, 8, 9 with NuGet caching |
| Publish | `v*` tag push | Tests, NuGet publish, GitHub Release with `.nupkg` |
| Release | Manual (workflow_dispatch) | Bump version, commit, tag, push (triggers Publish) |
| CodeQL | Push/PR + weekly | Security vulnerability scanning |
| Dependabot | Weekly | Opens PRs for NuGet + Actions updates |
| Auto-merge Dependabot | Dependabot PR | Auto-approves and squash-merges minor/patch bumps. Major bumps need manual review |
| Stale cleanup | Weekly (Sunday) | Marks issues/PRs stale after 60 days, closes after 14 more |

## Setup checklist (one-time, in GitHub repo settings)

### Allow auto-merge (Settings > General > Pull Requests)
- [x] Enable **"Allow auto-merge"**
- Required for the Dependabot auto-merge workflow to work

### Branch protection (Settings > Branches)
- Add rule for `main`
- Required status checks: `test (6.0.x)`, `test (8.0.x)`, `test (9.0.x)`
- Strict mode: branch must be up-to-date before merging
- Enforce admins: off (you can still push directly)
- Force pushes: blocked

### Secrets (Settings > Secrets and variables > Actions)
- `NUGET_API_KEY` — needed by publish.yml (already set)
