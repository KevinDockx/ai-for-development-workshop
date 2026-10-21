---
id: SEC-05
title: Password storage fails company policy
type: security
area: identity
priority: critical
size: L
ai-practice: [security-review, tests]
depends-on: []
status: open
---

## Story or problem statement

An identity review found that equal passwords produce the same short stored value for every account. The stored values do not meet company password-storage policy. Existing synthetic users must be migrated without forcing an immediate reset for everyone.

## Acceptance criteria

- [ ] Newly stored passwords meet the approved password-storage policy.
- [ ] Equal passwords no longer produce equal stored values.
- [ ] Existing valid users can sign in and are migrated through a documented transition.
- [ ] Invalid passwords remain rejected throughout the transition.
- [ ] No plain-text password or real credential appears in logs or tests.

## Notes

Use synthetic workshop accounts only when testing the migration path.

## AI approach hint

Ask AI to identify compatibility and migration cases first, then turn those cases into tests before changing behavior.
