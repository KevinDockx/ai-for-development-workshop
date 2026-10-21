---
id: DEB-05
title: Standardize error responses
type: tech-debt
area: cross-cutting
priority: high
size: L
ai-practice: [architecture, refactoring, tests]
depends-on: []
status: open
---

## Story or problem statement

Give ClaimHub consistent error responses based on Problem Details. Align the current exception, result, and null-return paths at the API boundary while preserving useful domain distinctions.

## Acceptance criteria

- [ ] Common validation, not-found, conflict, authorization, and unexpected errors have stable Problem Details responses.
- [ ] Internal exception detail is not returned outside development.
- [ ] Correlation information is available for unexpected failures.
- [ ] Representative controller and Minimal API paths behave consistently.
- [ ] Contract tests cover status, type, title, and relevant extensions.

## Notes

The internal programming model may still vary while the external API contract becomes consistent.

## AI approach hint

Ask AI to make an error matrix from observed responses before designing the common boundary.
