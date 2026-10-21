---
id: DEB-08
title: Standardize policyholder naming
type: tech-debt
area: cross-cutting
priority: medium
size: L
ai-practice: [refactoring, glossary]
depends-on: []
status: open
---

## Story or problem statement

Rename code that uses `Client` or `Customer` for the policy-owning party to the glossary term `PolicyHolder`. Preserve the distinct meaning of an insured person and avoid changing public contracts accidentally.

## Acceptance criteria

- [ ] The rename scope is based on the approved glossary.
- [ ] Policyholder and insured-person concepts remain distinct.
- [ ] Code, tests, and current documentation use the preferred term.
- [ ] Public contract changes are either avoided or documented and tested.
- [ ] The solution builds and tests with no stale internal aliases in scope.

## Notes

Search semantic usage, not just text, because some legacy names may describe different concepts.

## AI approach hint

Ask AI to classify each alias occurrence before bulk renaming it.
