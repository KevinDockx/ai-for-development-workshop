---
id: SEC-04
title: Secrets are committed to the repository
type: security
area: repository
priority: critical
size: M
ai-practice: [security-review, repository-audit]
depends-on: []
status: open
---

## Story or problem statement

Repository review found a signing value, a production-style connection string with credentials, and a payment API key in tracked content. The workshop values are fake, but the repository demonstrates the same exposure path as real credentials and encourages unsafe reuse.

## Acceptance criteria

- [ ] Secret scanning no longer reports credential values in the working tree.
- [ ] A clean checkout can be configured without committing a secret.
- [ ] Tracked examples contain obvious non-secret placeholders only.
- [ ] The response plan records that any value that had been real would require rotation.
- [ ] No real secret is introduced during verification.

## Notes

Treat the seeded values as compromised even though they are fictional.

## AI approach hint

Have AI inventory secret-shaped values and classify evidence, but review every proposed removal so required configuration names remain discoverable.
