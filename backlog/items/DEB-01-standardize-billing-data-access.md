---
id: DEB-01
title: Standardize Billing data access
type: tech-debt
area: billing
priority: medium
size: L
ai-practice: [architecture, refactoring]
depends-on: []
status: open
---

## Story or problem statement

Choose one data-access approach for new ClaimHub work and record the decision as an ADR. Then migrate Billing to demonstrate the choice. This gives future work a consistent example without pulling the complete repository into an unrelated rewrite.

## Acceptance criteria

- [ ] The ADR compares the approaches already present in ClaimHub.
- [ ] The decision states where the chosen approach applies and what remains out of scope.
- [ ] Billing uses the chosen approach consistently.
- [ ] Existing Billing behavior has regression coverage.
- [ ] Build and test commands pass after the migration.

## Notes

Keep the migration bounded to Billing so future work has a clear example to follow.

## AI approach hint

Ask AI to inventory current access patterns and their consequences before recommending one.
