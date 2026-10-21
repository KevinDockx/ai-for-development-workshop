---
id: MCP-03
title: Investigate SQLite through read-only MCP
type: mcp
area: cross-cutting
priority: high
size: M
ai-practice: [mcp, data-investigation, privacy]
depends-on: []
status: open
---

## Story or problem statement

Connect a SQLite MCP server in read-only mode and use it to investigate one data-related bug. Treat national numbers and IBANs as synthetic PII: minimize what you query, do not paste rows into external services, and do not retain personal fields in the report.

## Acceptance criteria

- [ ] The connection cannot modify the database.
- [ ] The investigation starts with schema and aggregate queries.
- [ ] Any row-level query selects only fields needed for the bug.
- [ ] The report contains no national numbers, IBANs, secrets, or full personal records.
- [ ] The observed data supports or disproves a specific bug hypothesis.

## Notes

The data is synthetic, but it still lets you practise the same careful handling you would use with real PII.

## AI approach hint

Start with counts and relationships. Narrow the query to individual rows only when the bug hypothesis requires that detail.
