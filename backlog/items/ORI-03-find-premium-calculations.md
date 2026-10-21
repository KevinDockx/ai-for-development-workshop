---
id: ORI-03
title: Find every premium calculation
type: orientation
area: policies
priority: high
size: S
ai-practice: [exploration]
depends-on: []
status: open
---

## Story or problem statement

Find every place in ClaimHub that calculates or recalculates a premium. The outcome is an inventory that shows which paths share domain logic and which paths duplicate it.

## Acceptance criteria

- [ ] Every premium calculation entry point is listed.
- [ ] Shared and duplicated calculation logic is distinguished.
- [ ] At least one representative call path is traced end to end.
- [ ] Differences in inputs or results are recorded without changing the code.

## Notes

Use `Premium-calculation.md` as a clue, not as proof.

## AI approach hint

Search by domain language as well as method names. AI may find calculations that do not use the word `premium`.
