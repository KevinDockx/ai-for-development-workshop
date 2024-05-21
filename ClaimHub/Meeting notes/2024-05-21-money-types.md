# Pricing implementation check-in

21 May 2024

**People:** Pieter Claes, Elise Van Acker, Bram Jacobs, Anouk Willems

Anouk’s instalment examples differ by one cent depending on where the calculation crosses from decimal to double. Pieter said an approximate type is fine for risk factors, but not for a monetary result. The current calculator carries doubles too far, so that boundary is not clear.

We agreed to represent money as `decimal` throughout ClaimHub: base premium, discount amounts, tax, assessed amount, limits, deductibles, invoices, refunds and payouts. Factors can be decimal too. There is no performance reason to make rating special.

Rounding is to cents per instalment, away from zero at the midpoint. The final instalment takes the remainder so the instalments equal the rounded total. The monthly schedule adds three percent before splitting.

This is the rule for new and corrected code. Bram noted that the first premium calculator still uses doubles internally. We will not quietly change it in the CH2 demo branch because we need pricing comparisons and tests first.

## To do

- Pieter: provide examples for annual and monthly schedules.
- Anouk: add a case where the final instalment absorbs one cent.
- Bram: create a follow-up for the premium calculator conversion.
- Elise: confirm the discount cap separately; this meeting did not decide it.
