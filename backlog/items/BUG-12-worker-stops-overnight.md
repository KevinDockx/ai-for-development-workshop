---
id: BUG-12
title: The worker stops overnight
type: bug
area: worker
priority: high
size: M
ai-practice: [debugging, tests]
depends-on: []
status: open
---

## Story or problem statement

**Reproduction:** Cause the nightly export operation to fail while its scheduled callback is running and observe the worker process and logs.

**Observed:** The failure cannot be observed through the scheduled operation, and the worker can terminate without a useful trace.

**Expected:** Scheduled failures are observed, logged once with useful context, and handled through the worker lifecycle without silent process loss.

## Acceptance criteria

- [ ] A test or controlled run reproduces an export failure.
- [ ] The failure is visible to the worker lifecycle.
- [ ] Logs contain the job identity and error without synthetic PII.
- [ ] Shutdown and cancellation are handled predictably.
- [ ] A later scheduled run follows the documented recovery behavior.

## Notes

Use a controlled synthetic failure rather than changing machine permissions during the workshop.

## AI approach hint

Ask AI to trace how completion and failure travel from the scheduled callback to the host.
