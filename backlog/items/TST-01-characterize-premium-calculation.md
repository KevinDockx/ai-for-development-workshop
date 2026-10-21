---
id: TST-01
title: Characterize premium calculation
type: test
area: policies
priority: high
size: M
ai-practice: [tests, agent]
depends-on: []
status: open
---

## Story or problem statement

Create characterization tests for premium and discount calculation before changing BUG-01 or BUG-05. Capture current behavior explicitly, including behavior known to be wrong, so later changes show exactly what moved.

## Acceptance criteria

- [ ] Tests cover annual totals, monthly schedules, and combined discounts.
- [ ] Boundary values that expose the two reported bugs are included.
- [ ] Tests that capture wrong current behavior are clearly labelled.
- [ ] Desired-behavior tests are kept distinct.
- [ ] The narrow test command and result are documented.

## Notes

Characterization protects the refactor path; it does not redefine the intended business rules.

## AI approach hint

Use the test-writer agent if available, and review whether each assertion observes behavior rather than implementation.
