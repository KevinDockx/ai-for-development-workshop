---
id: BUG-11
title: A payout can run twice
type: bug
area: claims
priority: critical
size: L
ai-practice: [debugging, concurrency-testing]
depends-on: []
status: open
---

## Story or problem statement

**Reproduction:** Submit two payout requests for the same approved claim at nearly the same time, or retry after an uncertain response.

**Observed:** Both requests reach the payment gateway and two completed payments are recorded.

**Expected:** A claim payout is executed at most once, and retries return the established outcome without issuing another payment.

## Acceptance criteria

- [ ] A concurrent regression test reproduces the duplicate payout.
- [ ] Only one gateway payment is made for one claim payout.
- [ ] A retry returns a consistent result without paying again.
- [ ] Competing requests leave the claim and payment records consistent.

## Notes

Payouts must be idempotent, with one stable identity per payout.

## AI approach hint

Ask AI to model two requests interleaving at each observable step, then turn the risky interleaving into a test.
