---
id: FEA-05
title: Download a policy schedule
type: story
area: documents
priority: medium
size: M
ai-practice: [planning, security-review, tests]
depends-on: []
status: open
---

## Story or problem statement

As a policyholder, I want to download the schedule for one of my policies, so that I have a portable record of its term, coverages, limits, deductible, and premium.

## Acceptance criteria

- [ ] A policyholder can download a schedule for a policy they own.
- [ ] A policyholder cannot download another person's schedule.
- [ ] A broker is limited to the documented portfolio scope.
- [ ] The schedule contains the current policy term and financial values.
- [ ] The generated document contains no unrelated personal data.

## Notes

Keep authorization behavior consistent with the policy ownership rules.

## AI approach hint

Define the document data contract and access matrix before choosing a rendering format.
