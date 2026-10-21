---
id: AIR-08
title: Add scoped AI instructions
type: ai-readiness
area: repository
priority: high
size: M
ai-practice: [instructions]
depends-on: []
status: open
---

## Story or problem statement

Add scoped instructions under `.github/instructions` for tests, API endpoints, EF Core, and Razor Pages. Each instruction must use `applyTo` so detail appears only when the agent is working on matching files.

## Acceptance criteria

- [ ] Separate scoped instructions exist for all four requested areas.
- [ ] Every file has a valid and appropriately narrow `applyTo` pattern.
- [ ] The instructions do not contradict repository-wide guidance.
- [ ] Each scope contains at least one verifiable ClaimHub-specific convention.
- [ ] Loading behavior is checked in the team's current client.

## Notes

Scoped instructions are useful when a rule matters in one part of the repository but would be noise everywhere else.

## AI approach hint

Give AI example paths from each area and ask whether each `applyTo` pattern would match them.
