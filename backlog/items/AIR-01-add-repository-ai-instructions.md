---
id: AIR-01
title: Add repository-wide AI instructions
type: ai-readiness
area: repository
priority: critical
size: M
ai-practice: [instructions]
depends-on: []
status: open
---

## Story or problem statement

Create concise repository-wide instructions that tell an AI coding assistant how to work safely in ClaimHub. Choose `.github/copilot-instructions.md`, the cross-IDE reference option, or `AGENTS.md`, and document why that home fits the clients your team actually uses.

## Acceptance criteria

- [ ] Exactly one repository-wide instruction home is chosen.
- [ ] The instructions cover authoritative commands, architecture constraints, business terminology, and verification.
- [ ] Instructions do not repeat detail better kept in scoped instructions or documentation.
- [ ] The choice and its client implications are documented.

## Notes

Keep always-on context short enough to remain useful on every task.

## AI approach hint

Ask AI to identify high-frequency rules, then remove anything that is task-specific or already enforced automatically.
