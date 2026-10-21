---
id: AGT-03
title: Create a characterization test writer
type: agent
area: cross-cutting
priority: medium
size: M
ai-practice: [agent, tests]
depends-on: []
status: open
---

## Story or problem statement

Create a test-writer agent that captures current behavior before refactoring. It must distinguish characterization from desired-behavior tests and run the narrowest relevant test set.

## Acceptance criteria

- [ ] The agent is stored under `.github/agents`.
- [ ] Its instructions require reading behavior before writing tests.
- [ ] It labels tests that intentionally capture undesirable current behavior.
- [ ] A trial run produces a focused test that fails if current behavior changes.
- [ ] The agent reports the command and result used for verification.

## Notes

A characterization test is a safety net, not an endorsement of existing behavior.

## AI approach hint

Give the agent one observable boundary and ask it to avoid coupling the test to private implementation detail.
