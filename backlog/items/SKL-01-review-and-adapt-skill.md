---
id: SKL-01
title: Review and adapt an existing skill
type: skill
area: repository
priority: high
size: M
ai-practice: [skills, supply-chain-review]
depends-on: []
status: open
---

## Story or problem statement

Find a skill that fits a ClaimHub need, review it, then adopt and adapt it under `.github/skills`. `github/awesome-copilot` is a GitHub-owned community catalogue, not a security-reviewed collection, so catalogue ownership is not a trust decision.

## Acceptance criteria

- [ ] The skill source and license are recorded.
- [ ] Included scripts and transitive behavior are read before execution.
- [ ] Requested tools, permissions, file access, and network access are reviewed.
- [ ] Assumptions are adapted to ClaimHub and the actual client.
- [ ] A bounded trial demonstrates the adapted skill's behavior.

## Notes

Treat downloaded customization content as code from a third party until you have reviewed it.

## AI approach hint

Ask AI to explain every script and permission, then verify that explanation against the files yourself.
