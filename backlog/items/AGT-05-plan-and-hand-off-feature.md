---
id: AGT-05
title: Plan and hand off a feature
type: agent
area: cross-cutting
priority: medium
size: M
ai-practice: [agent, planning, handoff]
depends-on: []
status: open
---

## Story or problem statement

Create a planner agent that produces a bounded feature plan and hands it to an implementer agent. Include the requirements, constraints, affected areas, and verification the implementer needs, without copying the complete planning conversation.

## Acceptance criteria

- [ ] Planner and implementer responsibilities are distinct.
- [ ] The planner does not edit implementation files.
- [ ] The handoff includes acceptance criteria and verification commands.
- [ ] A trial feature reaches the implementer with enough context to begin safely.
- [ ] Client-specific handoff behavior is documented.

## Notes

Repository agents belong under `.github/agents`; handoff buttons are client-dependent. When your client does not provide one, save the bounded handoff and invoke the implementer manually with that context.

## AI approach hint

Read the handoff as if you had not seen the planning conversation. If you could not start safely from that information, add the missing decision context and nothing else.
