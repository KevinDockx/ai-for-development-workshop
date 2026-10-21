---
id: BUG-05
title: Discounts can create a negative premium
type: bug
area: policies
priority: high
size: M
ai-practice: [debugging, tests]
depends-on: []
status: open
---

## Story or problem statement

**Reproduction:** Quote a loyal policyholder with a long claim-free history and enough active policies to receive every available discount.

**Observed:** Discounts accumulate beyond the permitted total and can produce a zero or negative premium.

**Expected:** Discounts combine multiplicatively, their effective total is capped at 35%, and the resulting premium remains valid before tax and payment adjustments.

## Acceptance criteria

- [ ] A regression test reproduces the excessive discount.
- [ ] Combined discounts use the documented multiplicative rule.
- [ ] The effective discount never exceeds 35%.
- [ ] Boundary cases at and beyond the cap return the expected premium.

## Notes

The no-claims, multi-policy, and loyalty discounts each retain their documented eligibility rules.

## AI approach hint

Ask AI to calculate the expected result independently from the current code before comparing intermediate values.
