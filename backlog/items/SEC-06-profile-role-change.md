---
id: SEC-06
title: Profile updates allow role changes
type: security
area: identity
priority: high
size: M
ai-practice: [security-review, tests]
depends-on: []
status: open
---

## Story or problem statement

A policyholder can add privileged account fields to an ordinary profile update. The submitted role, VIP flag, and credit limit are persisted even though those fields are not part of a self-service profile change.

## Acceptance criteria

- [ ] Self-service profile updates cannot change role, VIP status, or credit limit.
- [ ] Allowed profile fields can still be updated.
- [ ] Privileged changes require a separately authorized operation.
- [ ] Regression tests submit unexpected privileged fields and verify they have no effect.

## Notes

Test both the response and the stored record. A friendly response alone does not prove the fields were ignored.

## AI approach hint

Ask the reviewer to compare the user-visible form with the complete request surface.
