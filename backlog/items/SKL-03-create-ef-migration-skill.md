---
id: SKL-03
title: Create an EF Core migration skill
type: skill
area: repository
priority: medium
size: M
ai-practice: [skills, automation]
depends-on: []
status: open
---

## Story or problem statement

Create an `add an EF Core migration` skill under `.github/skills`. Include a script that uses the correct projects and makes migration creation and verification repeatable.

## Acceptance criteria

- [ ] The skill identifies the correct startup and data projects.
- [ ] The included script validates its inputs and failure codes.
- [ ] The procedure includes reviewing generated operations.
- [ ] Applying and rolling back a trial migration is documented.
- [ ] The trial leaves the repository in a clean, working state.

## Notes

Generated migrations still need review, especially when data could be lost.

## AI approach hint

Ask the skill to explain the generated migration before it applies anything.
