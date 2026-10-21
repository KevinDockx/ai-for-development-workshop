# ClaimHub workshop guide

For reference, the AI Enablement repository full of tips and tricks can be found here: https://dev.azure.com/CloudAndApplications/AI%20Infused%20and%20Augmented%20Factory/_git/AI%20Developer%20Enablement

## What you get

You have two things to work with today:

- `ClaimHub`, a deliberately imperfect .NET 10 insurance back-office application.
- `backlog`, a set of orientation, AI-readiness, agent, skill, MCP, security, bug, debt, feature, and test items.

Today, you will improve ClaimHub and make its repository easier and safer for AI to work in. Those two goals go together. Once the repository has better instructions, clearer domain documentation, and checks an agent can run, the AI has more useful context for the code changes you make later.

This is your copy. You can explore it, change it, and make mistakes in it. The code, people, policies, and credentials in the package are all synthetic.

## Set up your environment

You need:

- the .NET 10 SDK;
- a current Visual Studio 2026, or a current VS Code with C# Dev Kit;
- GitHub Copilot signed in; and
- local Git.

We'll use Windows PowerShell 5.1 as the shell throughout the workshop. From the `ClaimHub` folder, start by checking the versions of the tools you'll be using:

```powershell
dotnet --version
$PSVersionTable.PSVersion
git --version
```

Use these commands from the `ClaimHub` root:

```powershell
dotnet build .\ClaimHub.sln
dotnet test .\ClaimHub.sln
```

The solution test command does not include every test project on disk. That kind of mismatch is something you need to spot in brownfield code, so record what you find rather than hiding it.

### Run ClaimHub

Open three PowerShell terminals at the `ClaimHub` root and keep them there. Run one host in each terminal:

```powershell
dotnet run --project .\src\ClaimHub.Api
```

```powershell
dotnet run --project .\src\ClaimHub.Admin
```

```powershell
dotnet run --project .\src\ClaimHub.Worker
```

The launch settings use:

- API: `http://localhost:5263`
- Admin: `http://localhost:5264`
- API health check: `http://localhost:5263/health`
- OpenAPI document: `http://localhost:5263/openapi/v1.json`
- API reference UI: `http://localhost:5263/scalar/v1`

The Worker has no HTTP port. In Development, its five jobs run once after short staggered delays, so starting it changes local data and writes an export.

All three hosts resolve `data/claimhub.db` against the folder containing `ClaimHub.sln`, so the commands above use one shared SQLite database even though the hosts run from separate projects. You may start all three together. On the first start, one host initializes the database while the others may wait briefly and then skip seeding. That waiting is expected, so there is no need to routinely stop and restart the hosts. If a host reports an initialization timeout or failure, show the output to the trainer.

### Sign in with synthetic users

The following seeded users all use the workshop password `Passw0rd!`:

| Role | Login |
| --- | --- |
| Admin | `admin@scheldt.example` |
| Underwriter | `underwriter@scheldt.example` |
| Claims handler | `handler@scheldt.example` |
| Senior claims handler | `senior@scheldt.example` |
| Broker | `broker1@scheldt.example` |
| Broker | `broker2@scheldt.example` |
| Policyholder | `an.peeters@mail.example` |
| Policyholder | `jonas.maes@mail.example` |

These are not real credentials and they must not be reused anywhere else. Treat the synthetic data as though it were real while you practise secure engineering.

## Work the backlog

Work in your own copy, but do talk to the people around you as you go. You can compare approaches with a neighbour or pair on a difficult investigation. One person still owns the item, though, and commits the work in their own copy.

Start with the **Start here** lane in `backlog\README.md`:

1. `ORI-01`
2. `ORI-04`
3. `AIR-01`
4. `AIR-02`

When you take an item, update its front matter. There are exactly four valid statuses:

- `open`
- `in-progress`
- `blocked`
- `done`

Move an item to `in-progress` when you take ownership. Use `blocked` only when a real dependency prevents progress. Move it to `done` only after its acceptance criteria and relevant checks pass.

Commit often. A small local commit gives you something concrete to compare, review, or return to. You do not need a remote repository for that.

## A suggested strategy

### Understand before you change

Before asking AI to edit code, use it to explore the projects, runtime flow, Git history, and legacy documents. ClaimHub is brownfield code and several styles are in use. That means an answer based on one folder may tell you only part of the story.

### Make the repository AI-ready early

Work on repository instructions, maintained documentation, specifications, and executable scripts near the start of the day. The work you do there gives later tasks better context. Keep the always-on instructions short, though. Detailed or reusable knowledge belongs in scoped instructions, skills, or maintained documentation instead of being loaded for every task.

### Promote repetition

If you find yourself repeating the same useful approach, consider turning it into a skill or a focused agent. A prompt file may also fit when your client and execution mode support it, but prompt files depend on the client and harness, and they are deprecated for Agent Host sessions. For a new reusable workflow, I would therefore start with a skill.

Before writing a skill from scratch, inspect existing community material. `github/awesome-copilot` is a useful GitHub-owned catalogue, but an entry is still third-party content until you have reviewed it.

### Give AI executable feedback

Ask the agent to run a focused test, the build, or a read-only check. An explanation can sound completely convincing and still be wrong, so make sure the agent has a way to check its own work. For a large investigation, separate contexts can also help keep unrelated evidence from getting mixed together.

Finally, review every diff. AI can help produce the change, but you own what stays in your repository.

## Tips by AI practice

### Repository and scoped instructions

In the current repository, the repository instructions are in `.github/copilot-instructions.md`, and scoped instructions are in `.github/instructions`. These paths apply to clients that support these GitHub Copilot repository customizations; availability and behavior depend on the client and installed version.

Choose one clear home for short repository-wide guidance and explain the choice. Put rules that apply only to tests, API work, EF Core, or Razor Pages in scoped instruction files with accurate `applyTo` patterns. Check that each scoped file loads for an intended file and does not load for an unrelated one.

Avoid copying complete business documents into always-on instructions. Point to maintained documentation instead. That keeps the default context smaller and reduces the chance that two copies of the same rule drift apart.

### Custom agents

In the current repository, custom agents are in `.github/agents`. Use that path with clients that support repository custom agents; availability and behavior depend on the client and installed version.

Give a custom agent one recognizable responsibility and only the tools it needs for that responsibility. Decide whether you want it to explore, plan, review, or edit, and then try it on a bounded item. Look at the actions it took as well as the output it produced.

Do not treat an agent description as an enforcement boundary. Tool access, repository policy, review, and executable checks still matter.

### Subagents and `AGT-01`

Separate contexts can help when several investigations can proceed independently. Give each worker a narrow area and a common evidence format. A separate verifier can then challenge unsupported findings before the coordinator merges and deduplicates them.

`AGT-01` currently requires VS Code or Copilot CLI. Current Visual Studio documentation supports basic repository agents and skills, but not the coordinator, custom-subagent orchestration, and handoff behavior required by this item. If you are using Visual Studio, switch to VS Code or Copilot CLI for `AGT-01`, or pair with someone using one of those clients.

Client behavior changes over time. Record the client and date in your audit instead of assuming the same agent file behaves identically everywhere.

### Skills

In the current repository, skills are in `.github/skills`. Use that path with clients that support repository skills; availability and behavior depend on the client and installed version.

A useful skill describes a procedure you expect to repeat, gives the model the context it needs, and explains how to check the result. If an example makes a repository-specific pattern clearer, keep that example beside the skill.

Before adopting a skill from `github/awesome-copilot` or anywhere else, check:

- Who is the source, and can you trace the original contribution?
- What instructions, scripts, and bundled files will run?
- Which tools, file permissions, and network access does it expect?
- What paths, operating system, shell, and client does it assume?
- Does the license permit your intended use?
- Can you try it in a disposable branch or copy and review the resulting diff?

### MCP

An MCP server can be local, such as a process over the Markdown backlog, or remote, such as Microsoft Learn. Start with the smallest set of tools and data the task needs. Treat server output as external input, and pay attention to personal data, write access, and organization policy.

Configuration is client-specific:

- VS Code uses `.vscode/mcp.json`.
- Visual Studio uses solution-root `.mcp.json`.
- Copilot CLI uses `.github/mcp.json`.

The schemas are different, so do not copy one file into another location and assume it is portable. Relative commands also depend on the current workspace or working directory.

The Microsoft Learn MCP endpoint is:

`https://learn.microsoft.com/api/mcp`

It uses Streamable HTTP and does not require authentication, although network and organization policy may still block it.

### Model choice and cost

For bounded exploration or routine changes, start with a fast model. For architecture, security review, or a change with several constraints, consider a more capable reasoning model. This is a starting point rather than a fixed rule, so compare the result and tool behavior on your actual task.

Model availability, multipliers, quotas, and billing policy depend on the client, account, and organization, and they can change. Check the model picker and the current policy shown by your client rather than relying on workshop pricing notes.

## Definition of done

An item is done when:

- every acceptance criterion is met;
- the relevant build, tests, and focused checks pass;
- you reviewed the complete diff;
- affected documentation, ADRs, specifications, or glossary entries are current;
- no synthetic personal data or credential value has leaked into a report; and
- the backlog status is `done`.

Getting a generated change to compile may be useful progress. You still need to check the behavior and the complete diff before you call it finished.

## When you are stuck

Give yourself ten focused minutes first. You might narrow the question, inspect the evidence, try a smaller check, or start a fresh context with only the relevant files. If you are still blocked after ten minutes, ask the trainer. Bring the item ID, what you expected, what happened, and the evidence you already collected. That makes it much easier to help without taking the problem away from you.

## Optional inspiration

If you want a few ideas for agents, skills, instructions, or reusable prompts, have a look at [ClaimHub inspiration](student-inspiration.md). Use the examples to explore what might fit your work; none of them is a required workshop task.
