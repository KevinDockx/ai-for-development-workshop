---
id: FEA-02
title: Require four-eyes approval
type: story
area: claims
priority: critical
size: L
ai-practice: [planning, security-review, tests]
depends-on: []
status: open
---

## Story or problem statement

As a claims handler, I want payouts above €10,000 to require approval from a second senior claims handler, so that one person cannot authorize a high-value payout alone.

## Acceptance criteria

- [ ] Payouts at or below €10,000 follow the normal approval path.
- [ ] Payouts above €10,000 cannot proceed without a second approval.
- [ ] The second approver has the `SeniorClaimsHandler` role.
- [ ] The same person cannot provide both approvals.
- [ ] Both approval decisions and actors are auditable.

## Notes

The threshold is strictly above €10,000.

## AI approach hint

Ask AI to model roles, amount boundaries, and repeat requests before proposing the workflow.
