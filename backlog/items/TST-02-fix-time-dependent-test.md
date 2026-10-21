---
id: TST-02
title: Make the late-evening test deterministic
type: test
area: policies
priority: medium
size: S
ai-practice: [tests, debugging]
depends-on: []
status: open
---

## Story or problem statement

Make the test that fails between 23:00 and 00:00 local time deterministic. Its result must not depend on the wall clock, local time zone, or the moment the suite happens to run.

## Acceptance criteria

- [ ] The failing time window is explained by a reproducible test case.
- [ ] The test uses a controlled date and time.
- [ ] It passes in at least two simulated time zones.
- [ ] Repeated runs around the original boundary produce the same result.
- [ ] Production behavior is not weakened to make the test pass.

## Notes

Do not wait until late evening to verify this item.

## AI approach hint

Ask AI to list every source of current time and local-zone state used by the test path.
