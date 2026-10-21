---
id: BUG-02
title: Cancellation refunds are wrong
type: bug
area: policies
priority: high
size: M
ai-practice: [debugging, tests]
depends-on: []
status: open
---

## Story or problem statement

**Reproduction:** Cancel a policy halfway through its term, repeat with a leap-year term, and repeat after a claim has been paid in that term.

**Observed:** The ordinary cancellation can return zero or the wrong pro-rata amount, leap-year results use the wrong term length, and a positive refund is offered after a paid claim.

**Expected:** Unused premium is calculated from actual term days, while a paid claim in the current term results in no refund.

## Acceptance criteria

- [ ] Tests cover ordinary, leap-year, and paid-claim cancellation.
- [ ] Refunds use the actual number of days in the policy term.
- [ ] The amount is rounded according to the documented money rule.
- [ ] A paid claim in the current term produces no refund.

## Notes

Coverage periods use an exclusive end date.

## AI approach hint

Build a table of dates, used days, remaining days, and expected amounts before asking AI to diagnose the mismatch.
