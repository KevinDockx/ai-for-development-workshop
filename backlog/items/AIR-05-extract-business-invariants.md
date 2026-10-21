---
id: AIR-05
title: Extract business invariants
type: ai-readiness
area: cross-cutting
priority: high
size: L
ai-practice: [synthesis, documentation]
depends-on: []
status: open
---

## Story or problem statement

Extract the durable business rules buried in the legacy ticket export and turn them into domain documentation. Conflicting or incomplete tickets must remain visible rather than being silently combined into a new rule.

## Acceptance criteria

- [ ] Invariants cover policies, premiums, claims, and billing.
- [ ] Every invariant is traceable to one or more ticket keys.
- [ ] Conflicts and gaps are listed separately.
- [ ] Fictional workshop values are not presented as legal facts.
- [ ] The resulting documentation distinguishes intended behavior from current behavior.

## Notes

The source artifact is `scheldtjira-tickets-2025.csv`.

## AI approach hint

Have AI extract rules with ticket citations and confidence, then sample the source rows to check its interpretation.
