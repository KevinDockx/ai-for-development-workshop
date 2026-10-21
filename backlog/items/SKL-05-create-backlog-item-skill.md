---
id: SKL-05
title: Create a backlog item skill
type: skill
area: repository
priority: medium
size: M
ai-practice: [skills, documentation]
depends-on: []
status: open
---

## Story or problem statement

Create a skill under `.github/skills` that writes new ClaimHub backlog items in this backlog's format without exposing implementation solutions in bug or security reports.

## Acceptance criteria

- [ ] The skill emits every required front matter field.
- [ ] It validates enum values, IDs, dependencies, and lifecycle status.
- [ ] It selects the correct body form for stories, bugs, and security findings.
- [ ] Acceptance criteria are testable checkboxes.
- [ ] A trial item passes a manual format review.

## Notes

Use this backlog as the format reference. Do not copy a specific item's content as a universal template.

## AI approach hint

Test the skill with one bug and one feature so you can see whether it changes writing form correctly.
