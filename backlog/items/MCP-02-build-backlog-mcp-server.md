---
id: MCP-02
title: Build a backlog MCP server
type: mcp
area: repository
priority: high
size: L
ai-practice: [mcp, automation, tests]
depends-on: []
status: open
---

## Story or problem statement

Build a small MCP server over this Markdown backlog. It must list items, read one item, and update only its lifecycle status so an agent can use the backlog like a tracker.

## Acceptance criteria

- [ ] A list operation returns item IDs, titles, and statuses.
- [ ] A read operation returns one complete item by ID.
- [ ] An update operation accepts only `open`, `in-progress`, `blocked`, or `done`.
- [ ] Updating status preserves all other content and front matter.
- [ ] Invalid IDs and statuses return clear errors.
- [ ] Automated tests cover all three operations.

## Notes

VS Code uses `.vscode/mcp.json`; Visual Studio uses `.mcp.json`. Keep their configurations separate.

## AI approach hint

Define the narrow tool contracts first, then test updates against a temporary backlog copy.
