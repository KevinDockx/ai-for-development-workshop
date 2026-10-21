---
id: SEC-08
title: Refund and token authorization can be bypassed
type: security
area: billing
priority: critical
size: M
ai-practice: [security-review, tests]
depends-on: []
status: open
---

## Story or problem statement

An unauthenticated caller can reach refund processing. Separately, an expired token with the wrong issuer and audience is accepted as long as its signature is otherwise valid. Together, these findings show that sensitive billing behavior is not consistently protected by the expected identity checks.

## Acceptance criteria

- [ ] Anonymous refund requests are rejected before refund processing begins.
- [ ] Refund access is limited to the documented billing or administrative role.
- [ ] Expired tokens are rejected.
- [ ] Tokens with the wrong issuer or audience are rejected.
- [ ] A valid authorized refund flow still works.

## Notes

Use fake invoices and tokens only.

## AI approach hint

Review endpoint authorization and token acceptance as two separate boundaries, then test their combination.
