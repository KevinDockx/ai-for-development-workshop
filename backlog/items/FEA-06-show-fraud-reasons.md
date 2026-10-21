---
id: FEA-06
title: Show fraud flag reasons
type: story
area: admin
priority: medium
size: M
ai-practice: [planning, tests]
depends-on: []
status: open
---

## Story or problem statement

As a claims handler, I want to see why a claim was fraud-flagged in the Admin portal, so that I can investigate the signal without treating it as an automatic rejection.

## Acceptance criteria

- [ ] The portal shows each triggered fraud reason on the claim.
- [ ] Reasons cover claim frequency, early loss date, and high assessed amount.
- [ ] Unflagged claims show no invented reason.
- [ ] A flag does not automatically reject or change claim state.
- [ ] Displayed reasons are understandable without exposing unrelated PII.

## Notes

Fraud flags are informational. They never auto-reject a claim.

## AI approach hint

Ask AI to turn each documented flag rule into a named example before designing the display.
