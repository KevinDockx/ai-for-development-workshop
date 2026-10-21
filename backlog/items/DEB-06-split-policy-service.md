---
id: DEB-06
title: Split the policy service
type: tech-debt
area: policies
priority: medium
size: L
ai-practice: [refactoring, tests]
depends-on: []
status: open
---

## Story or problem statement

Split the large `PolicyService` into cohesive responsibilities without changing policy behavior. Callers should depend on the smallest useful capability rather than one service that handles every policy concern.

## Acceptance criteria

- [ ] Current responsibilities and callers are inventoried.
- [ ] Characterization tests protect representative behavior.
- [ ] Extracted services have cohesive names and bounded dependencies.
- [ ] Callers no longer require unrelated policy operations.
- [ ] The complete solution builds and tests after the refactor.

## Notes

Use business capabilities as boundaries rather than splitting the file by line count.

## AI approach hint

Ask AI to cluster methods by data, rules, and callers, then challenge any cluster named `helpers`.
