---
id: FEA-03
title: Add bicycle insurance
type: story
area: cross-cutting
priority: medium
size: L
ai-practice: [planning, agent, tests]
depends-on: []
status: open
---

## Story or problem statement

As a policyholder, I want to buy bicycle insurance and make a covered claim, so that my bicycle can be protected through ClaimHub alongside the existing products.

## Acceptance criteria

- [ ] Product rules, coverages, limits, deductibles, and rating inputs are documented.
- [ ] A bicycle policy can be quoted and activated.
- [ ] Premium, billing, and claim flows support the new product.
- [ ] Admin users can view and manage bicycle policies and claims.
- [ ] Tests cover one complete happy path and representative invalid inputs.

## Notes

This is intentionally cross-cutting. Do not invent business values without recording them as explicit workshop decisions.

## AI approach hint

Use an impact map across domain, API, data, admin, worker, tests, and docs before splitting implementation work.
