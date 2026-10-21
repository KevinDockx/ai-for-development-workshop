---
id: SEC-07
title: Logs and exports expose personal data
type: security
area: cross-cutting
priority: high
size: L
ai-practice: [security-review, privacy-review]
depends-on: []
status: open
---

## Story or problem statement

A privacy review found full synthetic IBANs and national registration numbers in routine information logs and an unprotected nightly export. In addition, any signed-in policyholder can request an export containing all policyholders and their personal data.

## Acceptance criteria

- [ ] Routine logs contain no full IBAN or national registration number.
- [ ] Nightly exports contain only justified fields and are protected according to the documented policy.
- [ ] Policyholders cannot export other policyholders' data.
- [ ] Authorized exports are auditable.
- [ ] Verification reports contain no full synthetic PII.

## Notes

The data is synthetic, but handle it as PII so the exercise reflects the real boundary.

## AI approach hint

Ask AI to create a data-flow inventory, then verify each sink and access boundary yourself.
