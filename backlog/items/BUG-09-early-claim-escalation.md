---
id: BUG-09
title: Claims are escalated too early
type: bug
area: worker
priority: high
size: M
ai-practice: [debugging, tests]
depends-on: []
status: open
---

## Story or problem statement

**Reproduction:** Report a claim on a Friday and evaluate escalation on the following Friday.

**Observed:** The claim is treated as seven days old and escalated even though only five business days have elapsed.

**Expected:** The first-assessment SLA counts business days, excludes weekends, and ignores public holidays for now.

## Acceptance criteria

- [ ] Tests cover a period that crosses a weekend.
- [ ] Weekend days do not count toward the SLA.
- [ ] The exact escalation boundary at five business days is documented and tested.
- [ ] Public holidays remain out of scope and are not silently approximated.

## Notes

Use fixed dates so the test result does not depend on the day it runs.

## AI approach hint

Ask AI to enumerate each calendar date and mark whether it counts before comparing the observed age.
