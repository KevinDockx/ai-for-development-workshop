---
id: FEA-04
title: Notify policyholders of claim changes
type: story
area: claims
priority: medium
size: M
ai-practice: [planning, tests]
depends-on: []
status: open
---

## Story or problem statement

As a policyholder, I want an email when my claim changes status, so that I know what happened without repeatedly checking ClaimHub.

## Acceptance criteria

- [ ] One notification is queued for each successful status change.
- [ ] Failed or rejected transitions send no notification.
- [ ] The message identifies the claim and new status without unnecessary PII.
- [ ] A retry does not send duplicate email for the same transition.
- [ ] Notification failure does not silently undo a completed claim transition.

## Notes

Use the fake email boundary supplied by the workshop; no real address or external service is needed.

## AI approach hint

Ask AI to list transaction and retry failure cases before deciding where notification is triggered.
