---
id: AIR-09
title: Clean up code-local context
type: ai-readiness
area: cross-cutting
priority: medium
size: M
ai-practice: [review, refactoring]
depends-on: []
status: open
---

## Story or problem statement

Remove comments that narrate obvious code or make claims the implementation does not support. Keep comments that explain intent, trade-offs, or constraints that cannot be expressed clearly in code.

## Acceptance criteria

- [ ] Misleading comments discovered during the review are removed or corrected.
- [ ] Obvious line-by-line narration is removed.
- [ ] Useful intent and constraint comments remain.
- [ ] Named or dated TODOs are converted to traceable work or removed.
- [ ] No behavior changes are introduced.

## Notes

The goal is reliable local context, not a repository with no comments.

## AI approach hint

Ask AI to classify comments by purpose and show you each proposed removal before applying it.
