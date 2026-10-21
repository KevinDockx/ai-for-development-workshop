---
id: SEC-10
title: Audit dependencies and serialization
type: security
area: cross-cutting
priority: high
size: M
ai-practice: [security-review, supply-chain-review]
depends-on: []
status: open
---

## Story or problem statement

A dependency and serialization audit found known vulnerable packages in the resolved graph. It also found that payment import and export accept or emit CLR type metadata even though the data format does not require it. Client-controlled input can therefore influence runtime type selection.

## Acceptance criteria

- [ ] `dotnet package list --vulnerable` is run and its dated result is recorded.
- [ ] Each reported advisory is assessed for reachability, severity, and licensing constraints.
- [ ] Client-controlled payment input cannot select runtime types.
- [ ] Exported data does not contain unnecessary CLR type metadata.
- [ ] Valid payment import and export scenarios still pass.

## Notes

Package audit output depends on restore state, configured sources, cache, and network access. Record those conditions rather than claiming an empty result proves safety.

## AI approach hint

Ask AI to separate package evidence, exploitability assumptions, and serialization behavior instead of jumping straight to an upgrade.
