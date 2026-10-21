---
id: MCP-01
title: Verify a .NET API with Microsoft Learn MCP
type: mcp
area: repository
priority: medium
size: S
ai-practice: [mcp, research]
depends-on: []
status: open
---

## Story or problem statement

Connect the Microsoft Learn MCP server at `https://learn.microsoft.com/api/mcp` and use it to verify a .NET 10 API before applying a fix.

## Acceptance criteria

- [ ] The endpoint is configured in the student's actual client.
- [ ] A .NET 10 API claim is checked against returned Microsoft Learn content.
- [ ] The source URL and retrieval date are recorded.
- [ ] The resulting code choice matches the verified API.
- [ ] Any organization-policy or availability limitation is recorded.

## Notes

Use separate MCP configuration files where needed: `.vscode/mcp.json` for VS Code and `.mcp.json` for Visual Studio.

## AI approach hint

Ask a narrow API question and require the agent to cite the documentation it used.
