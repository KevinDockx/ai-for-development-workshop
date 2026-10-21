---
id: BUG-07
title: Admin claims list is slow
type: bug
area: admin
priority: medium
size: M
ai-practice: [performance-analysis, verification]
depends-on: []
status: open
---

## Story or problem statement

**Reproduction:** Open the Admin claims list with several claims and capture the database queries for one page.

**Observed:** Loading related policy and policyholder data triggers an additional query for many individual rows, so query count grows with page size.

**Expected:** One bounded page should load with a small, stable number of queries while showing the same values.

## Acceptance criteria

- [ ] Before-and-after query counts are recorded for the same page.
- [ ] Query count no longer grows per displayed claim.
- [ ] The page still shows the required claim, policy, and policyholder values.
- [ ] Existing authorization and paging behavior remains unchanged.

## Notes

Use the diagnostics or profiling tools in your IDE, or ask AI to analyze query logs.

## AI approach hint

Give AI the query log and ask it to group repeated query shapes before proposing any change.
