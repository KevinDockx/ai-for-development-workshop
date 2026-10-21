---
id: BUG-03
title: Policy dates shift around boundaries
type: bug
area: policies
priority: high
size: M
ai-practice: [debugging, tests]
depends-on: []
status: open
---

## Story or problem statement

**Reproduction:** Create a policy when the UTC date differs from the Belgian calendar date, then calculate a one-year term starting on 29 February.

**Observed:** The created policy can start one calendar day early, and the leap-day term renews on the wrong date or loses a covered day.

**Expected:** Business dates follow the Belgian local calendar, timestamps remain UTC, and a 29 February policy ends and renews on 28 February under the documented exclusive-end convention.

## Acceptance criteria

- [ ] Tests reproduce both the local-date and leap-day failures.
- [ ] The same instant produces the intended Belgian business date.
- [ ] Ordinary one-year terms and 29 February terms have the documented boundaries.
- [ ] Coverage checks consistently treat the end date as exclusive.

## Notes

Keep calendar dates separate from timestamps in the test cases.

## AI approach hint

Ask AI to draw the same instant in UTC and Belgian local time before inspecting date conversions.
