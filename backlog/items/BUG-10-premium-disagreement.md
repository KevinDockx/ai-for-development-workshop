---
id: BUG-10
title: Admin and API premiums disagree
type: bug
area: admin
priority: high
size: M
ai-practice: [debugging, tests]
depends-on: []
status: open
---

## Story or problem statement

**Reproduction:** Recalculate the same monthly policy in the Admin portal and through the API, then compare the annual and installment amounts.

**Observed:** The Admin result differs because its monthly amount omits the 3% surcharge applied elsewhere.

**Expected:** Both entry points apply the same premium and installment rules and show the same result for the same inputs.

## Acceptance criteria

- [ ] A regression test compares both entry points with identical inputs.
- [ ] Annual totals and installment amounts agree.
- [ ] Monthly payment includes the documented surcharge.
- [ ] A future domain-rule change can be verified once for both entry points.

## Notes

This item exposes duplicated business behavior; DEB work may address the broader design.

## AI approach hint

Ask AI to trace both requests until their calculations diverge, then verify that divergence with a focused test.
