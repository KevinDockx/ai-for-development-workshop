---
id: SEC-03
title: Document downloads cross trust boundaries
type: security
area: documents
priority: critical
size: M
ai-practice: [security-review, tests]
depends-on: []
status: open
---

## Story or problem statement

A signed-in policyholder can download a document attached to somebody else's claim. The download request also accepts a crafted name that returns a file outside the configured document store. Both behaviors expose files beyond the caller's intended boundary.

## Acceptance criteria

- [ ] A caller cannot download a document belonging to another policyholder.
- [ ] A broker cannot cross portfolio boundaries.
- [ ] Crafted names cannot retrieve files outside the document store.
- [ ] Valid authorized downloads still return the expected file.
- [ ] Denied attempts are covered by regression tests.

## Notes

Use harmless synthetic files when reproducing the report.

## AI approach hint

Ask the reviewer to treat ownership and storage containment as separate claims that each need evidence.
