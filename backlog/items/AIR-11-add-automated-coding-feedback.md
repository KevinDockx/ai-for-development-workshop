---
id: AIR-11
title: Add automated coding feedback
type: ai-readiness
area: repository
priority: medium
size: M
ai-practice: [automation, verification]
depends-on: []
status: open
---

## Story or problem statement

Add `.editorconfig` and appropriate analyzers so contributors and AI agents receive fast, consistent feedback instead of having to infer formatting and code-quality expectations.

## Acceptance criteria

- [ ] `.editorconfig` applies consistently across the solution.
- [ ] Analyzer configuration is checked into the repository.
- [ ] Existing diagnostics are reviewed and handled deliberately.
- [ ] The solution builds in both Visual Studio and VS Code workflows.
- [ ] The chosen rules and any temporary exceptions are documented.

## Notes

Avoid turning on a large rule set and suppressing its output wholesale.

## AI approach hint

Introduce rules in small groups and ask AI to categorize the fallout before changing code.
