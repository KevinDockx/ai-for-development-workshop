---
id: TST-03
title: Restore API integration tests
type: test
area: repository
priority: high
size: L
ai-practice: [tests, verification]
depends-on: []
status: open
---

## Story or problem statement

Bring the orphaned API test project back into the solution, repair its outdated route expectation, and add integration tests for representative API behavior.

## Acceptance criteria

- [ ] The API test project is included in the solution.
- [ ] Its outdated route test reflects the current contract.
- [ ] Tests cover one authorized success, one authorization failure, and one validation failure.
- [ ] Test data and hosting are isolated and repeatable.
- [ ] The normal solution test command runs the integration tests successfully.

## Notes

Keep the suite useful on a local development machine; no external service should be required.

## AI approach hint

Ask AI to inspect why the project was excluded before changing the test or route.
