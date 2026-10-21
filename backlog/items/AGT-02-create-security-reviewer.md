---
id: AGT-02
title: Create a read-only security reviewer
type: agent
area: cross-cutting
priority: high
size: M
ai-practice: [agent, security-review]
depends-on: []
status: open
---

## Story or problem statement

Create a security reviewer agent with read-only tools and a ClaimHub-specific review checklist. Use it when working on each security backlog item, and keep implementation changes outside this agent.

## Acceptance criteria

- [ ] The agent is stored under `.github/agents`.
- [ ] Its available tools cannot edit files or run destructive actions.
- [ ] Its checklist covers authorization, input, secrets, PII, and dependencies.
- [ ] A report from one security item includes evidence and uncertainty.
- [ ] The report does not claim that review alone proves the application secure.

## Notes

Restricted tools make the review boundary visible, but you still need to inspect the result.

## AI approach hint

Ask for evidence a trainer can reproduce, not a list of generic security advice.
