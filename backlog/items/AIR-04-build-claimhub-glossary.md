---
id: AIR-04
title: Build the ClaimHub glossary
type: ai-readiness
area: cross-cutting
priority: high
size: M
ai-practice: [documentation]
depends-on: []
status: open
---

## Story or problem statement

Create a glossary that settles on `PolicyHolder` and explains how it differs from an insured person. Capture the competing terms used for policyholders, claims, and policies so readers and AI can interpret existing code without continuing the drift.

## Acceptance criteria

- [ ] `PolicyHolder` is defined as the preferred term.
- [ ] The distinction between policyholder and insured person is explicit.
- [ ] Known aliases for policyholder, claim, and policy are listed.
- [ ] Each preferred term matches the intended domain rules.
- [ ] Repository instructions link to the glossary.

## Notes

This item changes documentation first. Renaming code belongs to DEB-08.

## AI approach hint

Ask AI to inventory domain nouns, then have a human decide which terms express the business meaning.
