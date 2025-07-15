# Premium calculation

We use this rating summary for UAT comparisons. The rating spreadsheet signed off for a product release is still the reference.

## Sequence

Annual premium is calculated as:

```text
base rate x product risk factors
- customer discounts
+ insurance tax
```

The default tax in the ClaimHub test configuration is 9.25%. This is a programme assumption for testing, not a statement about the applicable legal rate.

## Discounts

| Discount | Rule |
|---|---|
| No claims | 5% per claim-free year, maximum 30% |
| Multi-policy | 10% with at least two active policies |
| Loyalty | 2% for each complete five years as a customer, maximum 6% |

The PolicyAdmin calculator adds the percentages and caps the result at 40%. Following the pricing review from May, CH2 is expected to combine them in sequence and cap the combined discount at 35%. We accept that difference during the parallel run.

## Instalments

Monthly payment attracts a 3% administration surcharge. Amounts are rounded to cents per instalment using normal commercial rounding. Put a rounding remainder on the final instalment so the schedule equals the rounded annual amount.

Quarterly remains available in PolicyAdmin for existing contracts, although it is not offered on new web quotes.

## Product inputs

Home uses postcode zone 1–4, building value, contents value and year of construction. Motor uses driver age, vehicle value, engine power in kW and claim-free years. Travel uses party type and geographical scope.

Use decimal amounts for all new rating work. There are still double values in the PolicyAdmin import and the first CH2 spike, so do not use those results as the accounting total.

_Pieter Claes, Pricing IT — 27 May 2024_
