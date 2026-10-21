---
id: MCP-04
title: Expose legacy tickets through MCP
type: mcp
area: repository
priority: low
size: M
ai-practice: [mcp, synthesis]
depends-on: []
status: open
---

## Story or problem statement

As a bonus, expose the legacy ticket export through a read-only MCP server and use it to support the invariant extraction in AIR-05.

## Acceptance criteria

- [ ] The server can search and read tickets without modifying the export.
- [ ] Multiline CSV fields are parsed correctly.
- [ ] Results retain ticket keys for traceability.
- [ ] Query output is bounded and excludes unnecessary reporter or assignee data.
- [ ] At least one documented invariant cites evidence retrieved through the server.

## Notes

The source artifact is `scheldtjira-tickets-2025.csv`.

## AI approach hint

Expose task-focused search and retrieval rather than a tool that dumps the complete export into context.
