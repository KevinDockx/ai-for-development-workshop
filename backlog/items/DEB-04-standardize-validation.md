---
id: DEB-04
title: Standardize validation
type: tech-debt
area: cross-cutting
priority: medium
size: L
ai-practice: [architecture, refactoring]
depends-on: []
status: open
---

## Story or problem statement

Choose one validation approach for ClaimHub and migrate a representative vertical slice. Make validation placement and error behavior predictable without mixing domain invariants with request-shape checks.

## Acceptance criteria

- [ ] Existing validation styles are inventoried.
- [ ] The chosen approach and scope are documented.
- [ ] One complete vertical slice uses it consistently.
- [ ] Invalid input produces a stable, tested response.
- [ ] Domain invariants remain enforced outside transport-only validation.

## Notes

Standardizing a tool is not enough; the repository also needs a clear boundary for each kind of rule.

## AI approach hint

Ask AI to classify current checks as transport, application, or domain validation before moving them.
