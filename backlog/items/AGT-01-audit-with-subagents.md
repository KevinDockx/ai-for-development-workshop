---
id: AGT-01
title: Audit repository health with subagents
type: agent
area: cross-cutting
priority: high
size: L
ai-practice: [agent, subagents, verification]
depends-on: []
status: open
---

## Story or problem statement

Set up a coordinator agent that audits policies, claims, billing, documents and identity, and admin and worker concerns in separate contexts. Include a cross-cutting security audit and a verifier that rejects findings without evidence. The coordinator then merges, deduplicates, and ranks the findings before writing the final report into the repository.

For this item, you need a client with separate-context custom-subagent support: current VS Code or Copilot CLI. If you use current Visual Studio, switch clients for this item or pair up with someone who can run it. Visual Studio is not claimed to run this orchestration.

## Acceptance criteria

- [ ] Every named area runs in a separate subagent context.
- [ ] A separate security review and evidence verifier run.
- [ ] The final report contains deduplicated, ranked findings with evidence.
- [ ] An unsupported finding is demonstrably rejected.
- [ ] The client and execution date are recorded in the report.

## Notes

Repository agents belong under `.github/agents`. Client capabilities may differ even when agent files can be discovered.

## AI approach hint

Give each worker a focused area and give the verifier explicit evidence criteria. That keeps one large context from trying to do the complete audit and makes unsupported findings easier to reject.
