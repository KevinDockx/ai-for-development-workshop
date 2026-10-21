---
id: TST-05
title: Add a local quality gate
type: test
area: repository
priority: high
size: M
ai-practice: [automation, tests, security-review]
depends-on: []
status: open
---

## Story or problem statement

Create one local quality-gate script that agents and people can run before they declare work done. It must build, test, check vulnerable packages, and verify formatting, with output that makes a failure easy to locate.

## Acceptance criteria

- [ ] The script runs build and the complete solution test suite.
- [ ] It runs `dotnet package list --vulnerable`.
- [ ] It performs a non-destructive formatting check.
- [ ] Any failed step produces a nonzero exit code and identifies the step.
- [ ] The documented command works from a clean checkout in Visual Studio and VS Code terminals.

## Notes

Package audit output can vary with network, sources, restore state, and cache, so the script should report those limitations clearly.

## AI approach hint

Keep the output compact so an agent can find the first failing step, but preserve the underlying command result needed to investigate it.
