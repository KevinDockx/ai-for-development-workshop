---
id: AIR-12
title: Review repository context quality
type: ai-readiness
area: repository
priority: high
size: M
ai-practice: [review, instructions]
depends-on: []
status: open
---

## Story or problem statement

Review the instructions, documentation, ADRs, and code for contradictions. Record what you find in a short context-quality report, then trim the always-on instructions so they point to details without repeating them.

## Acceptance criteria

- [ ] The review compares instructions, docs, ADRs, and representative code.
- [ ] Contradictions include evidence and an owner or resolution.
- [ ] Duplicate always-on guidance is removed or linked out.
- [ ] No unresolved conflict is presented as settled.
- [ ] The final repository-wide instructions remain concise.

## Notes

More context will not help when part of it is stale. One plausible outdated rule can send an agent in the wrong direction.

## AI approach hint

Have AI build a contradiction table, but inspect every cited source before you decide how to resolve it.
