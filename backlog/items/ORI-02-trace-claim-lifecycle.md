---
id: ORI-02
title: Trace the claim lifecycle
type: orientation
area: claims
priority: high
size: M
ai-practice: [exploration]
depends-on: []
status: open
---

## Story or problem statement

Trace the claim lifecycle from reporting through its final states. Record the allowed transitions, which roles may perform them, and where each business rule is currently enforced.

## Acceptance criteria

- [ ] The trace lists all current and intended claim states.
- [ ] Allowed transitions and final states are shown clearly.
- [ ] Each operation is linked to the role allowed to perform it.
- [ ] Each rule is traced to its current enforcement point, with gaps recorded.

## Notes

The legacy artifact `Claims-process.md` may help, but it is not automatically authoritative.

## AI approach hint

Have AI build a first trace from call paths, then compare it with the legacy documentation and observed behavior.
