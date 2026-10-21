---
id: BUG-04
title: Invalid claims can be approved
type: bug
area: claims
priority: critical
size: L
ai-practice: [debugging, tests]
depends-on: []
status: open
---

## Story or problem statement

**Reproduction:** Try to approve a closed claim, reject a paid-out claim, approve a claim after its policy was cancelled, report a loss before coverage starts, and report one exactly on the coverage end date.

**Observed:** These invalid transitions and out-of-coverage claims are accepted.

**Expected:** Final claim states remain final, only documented transitions are allowed, and the loss date must fall within an active policy's `[StartDate, EndDate)` period.

## Acceptance criteria

- [ ] Regression tests cover every reproduction case.
- [ ] Closed and withdrawn claims cannot leave their final state.
- [ ] Paid-out claims cannot be rejected.
- [ ] Approval requires valid active coverage for the loss date.
- [ ] A loss on the exclusive end date is rejected.

## Notes

Reopening is a separate future feature and does not change the current final-state rule.

## AI approach hint

Have AI turn the documented states into a transition table, then compare every observed transition with it.
