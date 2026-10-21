---
id: FEA-01
title: Reopen a closed claim
type: story
area: claims
priority: high
size: L
ai-practice: [planning, tests]
depends-on: [AIR-06]
status: open
---

## Story or problem statement

As a claims handler, I want to reopen a closed claim within 90 days when new evidence arrives, so that the evidence can be assessed without creating a disconnected replacement claim.

## Acceptance criteria

- [ ] An authorized claims handler can reopen an eligible closed claim within 90 days.
- [ ] Reopening requires a recorded reason and new-evidence reference.
- [ ] Claims outside the time window or in an ineligible state remain unchanged.
- [ ] The previous final state, actor, reason, and timestamp are auditable.
- [ ] The resulting state follows the approved current specification.

## Notes

Implement against the specification produced by AIR-06, not `claim-reopening-v0.md`.

## AI approach hint

Ask AI to derive examples and boundary cases from the approved spec before planning the workflow change.
