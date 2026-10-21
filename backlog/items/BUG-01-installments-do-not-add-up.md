---
id: BUG-01
title: Installments do not add up
type: bug
area: billing
priority: high
size: M
ai-practice: [debugging, tests]
depends-on: []
status: open
---

## Story or problem statement

**Reproduction:** Create a monthly schedule for an annual amount that does not divide evenly into twelve cents-based installments, such as 100.01, and total the installments.

**Observed:** The installment sum differs by one cent from the displayed annual total with surcharge.

**Expected:** Currency calculations use the documented money rules, and all installments add up exactly to the rounded annual total.

## Acceptance criteria

- [ ] A regression test reproduces the one-cent difference.
- [ ] The sum of installments equals the annual total for representative boundary amounts.
- [ ] Installments round to cents using `MidpointRounding.AwayFromZero`.
- [ ] Any remainder is reflected in the final installment.

## Notes

Money is `decimal`; monthly payment adds a 3% surcharge.

## AI approach hint

Ask AI to follow one amount through every intermediate calculation and compare values before and after rounding.
