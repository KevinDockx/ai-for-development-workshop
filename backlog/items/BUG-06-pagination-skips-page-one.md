---
id: BUG-06
title: Claims pagination skips page one
type: bug
area: claims
priority: medium
size: S
ai-practice: [debugging, tests]
depends-on: []
status: open
---

## Story or problem statement

**Reproduction:** Request page 1 of claims with a page size of 10 and compare it with the first ten claims in the ordered data set.

**Observed:** The response starts with the eleventh claim, so the first page is never returned.

**Expected:** Page 1 contains the first page-size records, page 2 contains the next records, and response metadata follows the same one-based convention.

## Acceptance criteria

- [ ] A regression test reproduces the skipped first page.
- [ ] Page 1 returns the first ordered records.
- [ ] Adjacent pages neither overlap nor leave a gap.
- [ ] Invalid page numbers are handled consistently.

## Notes

Define ordering in the test so database row order does not make it flaky.

## AI approach hint

Ask AI to write out record indexes for pages 1 and 2 before inspecting the observed offset.
