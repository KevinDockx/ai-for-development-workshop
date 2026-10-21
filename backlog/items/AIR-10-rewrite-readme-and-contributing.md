---
id: AIR-10
title: Rewrite README and CONTRIBUTING
type: ai-readiness
area: repository
priority: high
size: M
ai-practice: [documentation, verification]
depends-on: []
status: open
---

## Story or problem statement

Rewrite README and CONTRIBUTING so a new contributor gets current setup commands, architecture pointers, and conventions instead of guidance that contradicts the repository.

## Acceptance criteria

- [ ] Setup commands, ports, projects, and prerequisites match a verified run.
- [ ] CONTRIBUTING points to current architecture and domain guidance.
- [ ] Stale mandates about data access, endpoint style, dates, serialization, and mapping are removed.
- [ ] The documents do not duplicate detailed scoped instructions.
- [ ] Every documented command has been executed successfully.

## Notes

Use the evidence recorded in ORI-04 if it is available, but ORI-04 is not a prerequisite.

## AI approach hint

Let AI compare the documents with the repository, then verify every claimed fact before keeping it.
