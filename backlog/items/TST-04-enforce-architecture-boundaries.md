---
id: TST-04
title: Enforce architecture boundaries
type: test
area: repository
priority: medium
size: M
ai-practice: [tests, architecture]
depends-on: []
status: open
---

## Story or problem statement

Add architecture tests that turn ClaimHub's intended project and layer boundaries into executable feedback for contributors and AI agents.

## Acceptance criteria

- [ ] Intended dependency directions are documented.
- [ ] Tests cover Domain, Infrastructure, API, Admin, and Worker boundaries.
- [ ] At least one deliberately introduced invalid dependency makes a test fail during verification.
- [ ] Exceptions are explicit and justified.
- [ ] The tests run as part of the normal solution test command.

## Notes

Test actual agreed boundaries, not an idealized architecture the repository has never adopted.

## AI approach hint

Start from project references and ADRs, then ask AI to identify only the constraints with clear evidence.
