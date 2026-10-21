---
id: BUG-08
title: Billing creates duplicate invoices
type: bug
area: billing
priority: critical
size: M
ai-practice: [debugging, tests]
depends-on: []
status: open
---

## Story or problem statement

**Reproduction:** Run the billing job twice for the same business date and inspect invoices for the same policy installment.

**Observed:** The second run creates another invoice for an installment that was already invoiced.

**Expected:** Repeating the same billing run is safe and leaves one invoice per installment.

## Acceptance criteria

- [ ] A regression test runs the same billing date twice.
- [ ] The second run creates no duplicate invoice.
- [ ] Different legitimate installments still receive separate invoices.
- [ ] Concurrent or retried execution cannot create two invoices for the same business event.

## Notes

The business rule is one invoice per installment.

## AI approach hint

Ask AI to identify the stable business identity of an invoice before examining retry behavior.
