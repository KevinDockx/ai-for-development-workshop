---
id: SEC-09
title: Admin portal accepts unsafe navigation and content
type: security
area: admin
priority: high
size: M
ai-practice: [security-review, tests]
depends-on: []
status: open
---

## Story or problem statement

Claim descriptions containing active markup execute when an administrator opens the details page. The login flow also redirects a signed-in user to an external address supplied in the return location. Both behaviors let untrusted input control a trusted browser interaction.

## Acceptance criteria

- [ ] A claim description is displayed as content without executing active markup.
- [ ] Normal text remains readable.
- [ ] External return locations are rejected after login.
- [ ] Valid local return locations still work.
- [ ] Both findings have regression coverage.

## Notes

Use a harmless proof string and an invalid example domain during reproduction.

## AI approach hint

Ask the security reviewer to identify browser trust boundaries and produce separate evidence for rendering and navigation.
