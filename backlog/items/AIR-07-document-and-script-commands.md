---
id: AIR-07
title: Document and script repository commands
type: ai-readiness
area: repository
priority: high
size: M
ai-practice: [automation, verification]
depends-on: []
status: open
---

## Story or problem statement

Document the authoritative build, test, and run commands, and provide scripts that an agent can execute without guessing project names or working directories. Repair the broken build script as part of that work.

## Acceptance criteria

- [ ] Build, test, API, Admin, and Worker commands are documented.
- [ ] Scripts return a nonzero exit code when a step fails.
- [ ] The broken build script no longer references removed projects.
- [ ] Commands work from the documented directory in Visual Studio and VS Code terminals.
- [ ] An AI agent can run the scripts without interactive input.

## Notes

Prefer a small set of authoritative entry points over several nearly identical scripts.

## AI approach hint

Ask AI to execute the documented path mentally first, then verify every command in a clean terminal.
