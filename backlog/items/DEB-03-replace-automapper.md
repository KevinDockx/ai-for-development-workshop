---
id: DEB-03
title: Replace AutoMapper deliberately
type: tech-debt
area: cross-cutting
priority: high
size: L
ai-practice: [architecture, refactoring, research]
depends-on: []
status: open
---

## Story or problem statement

Decide what replaces AutoMapper in ClaimHub, record that decision in an ADR, and implement the chosen direction. Account for current mapping styles, migration effort, testability, and the package's licensing and security constraints.

## Acceptance criteria

- [ ] The ADR compares realistic mapping options and their migration cost.
- [ ] Licensing and current advisory evidence are dated and cited.
- [ ] The chosen approach is demonstrated across representative mappings.
- [ ] Existing API contracts remain covered by tests.
- [ ] No obsolete mapping path remains in the migrated scope.

## Notes

AutoMapper 14.0.0 was the final MIT-licensed release before the licensing change, and current advisory facts must be rechecked when doing the work.

## AI approach hint

Ask AI to separate product facts from architecture preferences and cite primary sources for the former.
