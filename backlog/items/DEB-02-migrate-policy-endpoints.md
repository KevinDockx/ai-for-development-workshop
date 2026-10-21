---
id: DEB-02
title: Migrate policy endpoints
type: tech-debt
area: policies
priority: medium
size: L
ai-practice: [refactoring, tests]
depends-on: []
status: open
---

## Story or problem statement

Migrate `PoliciesController` to Minimal APIs in line with the accepted `migrate when touched` decision. Preserve the external contract unless a separately documented correction is required.

## Acceptance criteria

- [ ] All policy routes handled by `PoliciesController` have equivalent Minimal API endpoints.
- [ ] Authorization, validation, status codes, and response shapes are preserved.
- [ ] Contract or integration tests cover the migrated routes.
- [ ] The old controller is removed after parity is proven.
- [ ] OpenAPI output remains accurate.

## Notes

The decision does not require unrelated controllers to move in this item.

## AI approach hint

Have AI produce a route-contract checklist from the current controller before generating endpoints.
