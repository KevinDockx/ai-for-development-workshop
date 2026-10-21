---
id: AGT-06
title: Coordinate cross-cutting work
type: agent
area: cross-cutting
priority: low
size: L
ai-practice: [agent, planning]
depends-on: []
status: open
---

## Story or problem statement

As a bonus, create a coordinator for a change that crosses several areas, such as bicycle insurance or the AI-readiness migration. It should divide work by responsibility while keeping one shared set of domain decisions. The separate pieces may run sequentially when your client does not support separate-context workers.

## Acceptance criteria

- [ ] The chosen work spans at least three repository areas.
- [ ] Worker boundaries and shared constraints are explicit.
- [ ] Conflicting worker proposals are detected and resolved.
- [ ] The coordinator produces one integrated plan or change set.
- [ ] Final verification covers all affected areas.

## Notes

Do not introduce parallel agents when the work is small enough for one clear context.

## AI approach hint

Separate independent investigation from shared design decisions, and reconcile before implementation diverges.
