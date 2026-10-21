---
id: SKL-04
title: Create an audit checklist skill
type: skill
area: cross-cutting
priority: medium
size: M
ai-practice: [skills, security-review]
depends-on: []
status: open
---

## Story or problem statement

Create an audit checklist skill under `.github/skills` for the area workers used by AGT-01. It must produce evidence-based findings while allowing an area to report that no issue was proven.

## Acceptance criteria

- [ ] The skill covers correctness, security, maintainability, tests, and context.
- [ ] Findings require reproducible evidence and a confidence statement.
- [ ] The checklist distinguishes symptoms from proposed remediation.
- [ ] One area audit demonstrates the output format.
- [ ] Unsupported findings can be marked rejected by a verifier.

## Notes

The checklist should focus attention without forcing every auditor to report the same generic problems.

## AI approach hint

Use questions and evidence requirements rather than a list of expected answers.
