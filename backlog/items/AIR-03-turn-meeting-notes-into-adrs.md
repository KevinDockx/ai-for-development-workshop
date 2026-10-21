---
id: AIR-03
title: Turn meeting notes into ADRs
type: ai-readiness
area: repository
priority: high
size: L
ai-practice: [documentation, synthesis]
depends-on: []
status: open
---

## Story or problem statement

Convert the decisions hidden in the meeting notes into a coherent ADR set. The history must remain visible, including at least one superseded decision and one rejected option.

## Acceptance criteria

- [ ] Every confirmed architectural decision in the meeting notes has an ADR.
- [ ] ADRs include context, decision, status, and consequences.
- [ ] Superseded ADRs link to their replacement.
- [ ] The rejected event-sourcing option is recorded as rejected.
- [ ] Claims in each ADR can be traced to meeting-note evidence.

## Notes

Do not rewrite history to make the repository look more consistent than it is.

## AI approach hint

Use AI to extract candidate decisions, but compare each ADR with the original note before setting its status.
