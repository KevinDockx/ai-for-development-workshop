---
id: SEC-01
title: Crafted claim search exposes unintended data
type: security
area: claims
priority: critical
size: M
ai-practice: [security-review, tests]
depends-on: []
status: open
---

## Story or problem statement

A penetration test found that a crafted claim-search term changes which records the search returns. The response includes claims that do not match the intended query, including a claim that should no longer appear in normal results. This allows a caller to cross the search boundary and disclose data.

## Acceptance criteria

- [ ] A safe regression test reproduces the boundary bypass without using real data.
- [ ] Crafted input is treated as search text and cannot alter query meaning.
- [ ] Claims excluded from normal results remain excluded.
- [ ] Valid searches continue to return their expected matches.

## Notes

Use only synthetic workshop data and avoid copying attack payloads into logs or reports.

## AI approach hint

Ask a read-only reviewer to trace untrusted search input to the data boundary and require reproducible evidence.
