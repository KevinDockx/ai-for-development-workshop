---
id: DEB-07
title: Enable nullable and warnings as errors
type: tech-debt
area: repository
priority: medium
size: L
ai-practice: [refactoring, verification]
depends-on: []
status: open
---

## Story or problem statement

Enable nullable reference analysis and warnings as errors in Infrastructure, then fix the resulting diagnostics without broad suppression. The project should give people and agents trustworthy compiler feedback.

## Acceptance criteria

- [ ] Nullable analysis is enabled for Infrastructure.
- [ ] Warnings are treated as errors in the project's normal build.
- [ ] Existing broad `NoWarn` entries are removed or narrowly justified.
- [ ] Nullability annotations match runtime behavior.
- [ ] The solution builds with no Infrastructure warnings.

## Notes

Do not silence a warning merely to reach a green build.

## AI approach hint

Group diagnostics by root cause and fix one category at a time.
