---
id: SEC-02
title: Users can view policies outside their scope
type: security
area: policies
priority: high
size: M
ai-practice: [security-review, tests]
depends-on: []
status: open
---

## Story or problem statement

A policyholder can request another policyholder's policies by changing an identifier. A broker can do the same for a policyholder outside that broker's portfolio. The service returns policy data instead of denying access.

## Acceptance criteria

- [ ] A policyholder cannot view policies owned by another policyholder.
- [ ] A broker cannot view policies outside their own portfolio.
- [ ] Authorized staff access remains explicit and tested.
- [ ] Denied requests do not disclose whether the target record exists.

## Notes

Test with separate synthetic accounts representing each role and ownership boundary.

## AI approach hint

Build a small role-and-ownership matrix before asking AI to review the observed access decisions.
