# Release Process

This document defines a consistent, low‑risk release workflow for the FTdx101 WebApp.

## Goals
Ensure stable releases, prevent architectural drift, and maintain predictable behaviour across versions.

## Branching
- main: stable, production-ready.
- develop: integration branch.
- feature/*: isolated work.
- hotfix/*: urgent fixes.

## Preconditions
Before merging to develop or main:
- All tests pass.
- No drift detected.
- Architecture-checklist.md completed.
- Documentation updated.
- Copilot-compliance-test.md validated.

## Versioning
- Semantic versioning: MAJOR.MINOR.PATCH.
- MAJOR: breaking changes.
- MINOR: new features.
- PATCH: fixes only.

## Release Steps
1. Finalise changes in develop.
2. Run full drift-check.
3. Update version number.
4. Update changelog.
5. Merge develop → main.
6. Tag release.
7. Build and deploy.

## Hotfix Flow
1. Branch from main.
2. Apply fix.
3. Test and validate.
4. Merge hotfix → main.
5. Merge hotfix → develop.
6. Tag patch release.

## Changelog Rules
- Keep entries short and factual.
- Group by Added/Changed/Fixed.
- Reference issues or PRs where relevant.

## Deployment
- Build must be reproducible.
- No manual edits post-build.
- Validate UI performance after deployment.

## Post‑Release
- Monitor logs and UI behaviour.
- Document any regressions.
- Plan follow-up fixes if needed.

## Summary
Stable releases require strict validation, drift prevention, consistent versioning, and a predictable workflow.
