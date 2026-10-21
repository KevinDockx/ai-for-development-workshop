# ClaimHub inspiration

Everything on this page is optional. There is no extra checklist or backlog to complete. Pick an idea when it helps with the ClaimHub work you are doing, and leave it alone when it does not.

## Customizations: do not reinvent the wheel

You do not need to build every customization yourself. A useful setup will normally combine reviewed agents and skills that already exist in your client, organization, or community with a few customizations for this project.

GitHub's [`github/awesome-copilot`](https://github.com/github/awesome-copilot) is one useful catalogue. Before you adopt an entry, though, review its source, instructions, tools, bundled scripts, permissions, assumptions, and license. When it makes sense, try it in a disposable branch or copy so you can inspect what it actually does.

As a rule of thumb:

- Adopt or adapt a general capability such as code review, test generation, ADR writing, or security review.
- Build a customization when it needs ClaimHub terminology, architecture, business rules, evidence sources, or scripts.
- Do neither when a clear one-time prompt is enough.

Each customization type has a different job:

- **Instructions** provide short, stable guidance that should apply across the repository.
- **Scoped instructions** load guidance only for matching files, such as tests or API code.
- **Skills** describe a reusable procedure, including when to use it, what evidence it needs, and how to check the result.
- **Scripts within skills** perform deterministic work that should produce the same kind of result each time.
- **Agents** take a focused responsibility. They choose and coordinate tools or skills, stay within their boundaries, and interpret the evidence.

## Instruction ideas

Use these ideas to decide what belongs in your own instruction files. They are deliberately not complete, ready-made files: inspect ClaimHub first, then write only the guidance the repository evidence supports.

### Repository-wide ClaimHub instructions

Keep the always-on file short. For ClaimHub, it could identify the solution and host layout, the shared SQLite database, the supported PowerShell shell, the expected build and test entry points, and the maintained sources for business terminology. It could also tell the agent to handle synthetic data as though it were real and to report failed checks rather than hide them.

Do not copy complete business rules into this file. Point to their maintained source instead. Otherwise, you have a second copy that will eventually drift.

### API-scoped instructions

For API guidance, you could cover ClaimHub's mix of controllers and Minimal APIs, its authentication and authorization conventions, route and response compatibility, validation, error responses, pagination, and OpenAPI expectations. Include the representative API tests and a realistic request check the agent should run.

The scope should match API files and should not load for an unrelated Razor Page or Worker job.

### EF Core and persistence instructions

For persistence guidance, explain the shared SQLite model, where the `DbContext`, mappings, migrations, and initialization code live, and how all three hosts resolve the database. Depending on what you find, it may also need to cover transaction boundaries, concurrency expectations, money precision, relationship loading, and the checks required after a schema or query change.

Base those rules on the implementation you find. Do not turn a pattern from one repository area into a repository-wide rule without checking the other paths.

### Test instructions

Test guidance could describe the test framework and naming conventions, where unit and integration tests belong, and how ClaimHub resets or isolates mutable data. Ask for tests of observable behavior rather than implementation details, and identify the focused test command to run before broader validation.

ClaimHub also gives you a reason to compare the solution file with the test projects on disk. An instruction should not assume that the normal solution command discovers everything unless you have verified that it does.

### Razor Pages instructions

For Razor Pages, the guidance could cover the Admin application's page, handler, model-binding, validation, authorization, antiforgery, and error-display conventions. It may also identify shared UI patterns and the browser behavior that needs checking after a change.

Keep API or Worker details out of this scope unless a Razor Page actually depends on them.

## Project-specific skill ideas

Now we can make this specific to ClaimHub. The following workflows depend on its concepts or repository evidence, which makes them better candidates for project-specific skills.

When work must be deterministic and repeatable, put that part in a script instead of asking an LLM to improvise it on every run. A skill can coordinate and explain the script, while an agent can decide when to invoke it and interpret the evidence. The script then runs the same checks in the same way each time.

### Compare premium calculations

```text
.github/skills/compare-premiums/
├── SKILL.md
├── compare-premiums.ps1
└── scenarios.json
```

The script would run fixed motor, home, and travel scenarios across the relevant calculation paths. It would capture the calculation components, compare money using an explicit tolerance, produce stable output, and fail when it finds an unexpected difference.

That split matters. The LLM can investigate why two paths differ, but the script proves whether they differ and where the first difference appears. The approved business rules still determine which result is correct.

Be careful when you choose the baseline. The current behavior may already be flawed, and declaring today's output correct could preserve the defect you are trying to find.

### Audit generated exports for sensitive data

```text
.github/skills/audit-exports/
├── SKILL.md
├── inspect-export.ps1
└── allowed-fields.json
```

The script would check headers and fields against the approved list, then look for candidate email addresses, national numbers, IBANs, tokens, and credentials. It should report locations without printing full values and return a machine-readable result that another tool can evaluate.

Expect false positives. Pattern matching can identify data that needs investigation, but a clean pattern scan is not proof that an export is safe.

### Detect API contract drift

```text
.github/skills/detect-api-contract-drift/
├── SKILL.md
├── compare-openapi.ps1
└── approved-openapi.json
```

The script would retrieve and normalize the current OpenAPI document, then compare routes, methods, parameters, request bodies, responses, and schemas. It should report compatible additions separately from potentially breaking changes. That way, a large diff does not hide the change you need to investigate.

An approved contract change requires an explicit baseline update. The script should never update `approved-openapi.json` merely because the current application produced something different.

### Reproduce a claim lifecycle

```text
.github/skills/reproduce-claim-lifecycle/
├── SKILL.md
├── invoke-claim-lifecycle.ps1
└── scenarios/
    ├── standard-claim.json
    └── payout-claim.json
```

The script would authenticate a synthetic user, report a fixed claim, retrieve it, and move it through defined transitions. It would check the timeline and, when the scenario calls for it, the payout. Along the way it would verify status codes and entity relationships, and record created IDs so the run can be cleaned up or investigated.

Keep the scenarios fixed enough to reproduce a failure. If the script invents a different lifecycle on every run, you cannot easily compare the evidence from one run with the next.

### Check backlog integrity

```text
.github/skills/check-backlog/
├── SKILL.md
└── test-backlog.ps1
```

The script would validate item IDs, prefixes, statuses, required front matter, dependency references and cycles, board links, acceptance headings, and filenames. These are mechanical rules, so a deterministic check is a better fit than interpreting them again on every run.

## Project-specific agent ideas

Each of these agents has one focused responsibility. The notes give you useful boundaries, evidence, and output to start from; they are not complete agent files.

### Premium discrepancy investigator

- **Responsibility:** Investigate one discrepancy across premium and rating paths.
- **Boundaries:** Read-only. It does not change formulas, tests, or accepted baselines.
- **Key evidence:** Calculation inputs and components, approved rating rules, relevant history, characterizing tests, and the Compare premium calculations skill.
- **Representative output:** The first divergence, the authority for the expected rule, confidence in the finding, and the smallest characterizing test that would expose it.

### Claim lifecycle integrity auditor

- **Responsibility:** Determine whether a claim lifecycle can become impossible, unauthorized, or financially inconsistent.
- **Boundaries:** Read-only. It inspects transitions, history, eligibility, reserves, coverage, fraud checks, authorization, payout, and concurrency without approving or paying a real claim.
- **Key evidence:** Domain rules, handlers, persistence behavior, authorization checks, tests, and results from the Reproduce a claim lifecycle skill.
- **Representative output:** A reproducible lifecycle path, the violated invariant or authorization boundary, financial impact, confidence, and the smallest decisive follow-up check.

### Billing reconciliation investigator

- **Responsibility:** Reconcile one billing discrepancy across installments, invoices, payments, credit notes, refunds, dunning, renewal, and Worker processing.
- **Boundaries:** It investigates and reports. It does not post payments, issue refunds, or rewrite financial records.
- **Key evidence:** Related entity records, transaction boundaries, Worker execution, idempotency controls, logs, and billing tests.
- **Representative output:** A timeline of financial events, duplicate or missing effects, the first inconsistent state, likely cause, and a repeatable verification plan.

### Cross-host impact analyst

- **Responsibility:** Map the effects of a proposed change across API, Admin, Worker, and shared code.
- **Boundaries:** Read-only. It does not implement the change or expand the requested scope.
- **Key evidence:** Shared SQLite access, domain and contract dependencies, startup behavior, scheduled jobs, exports, and affected tests.
- **Representative output:** An impact map by host, shared assumptions at risk, checks required before merging, and areas with no demonstrated effect.

### Legacy evidence archaeologist

- **Responsibility:** Investigate one disputed rule or historical decision.
- **Boundaries:** It answers the bounded question and does not treat old documentation or current code as automatically authoritative.
- **Key evidence:** Executable behavior, tests, maintained and legacy documents, Git history, ADRs, and issue context.
- **Representative output:** Evidence classified as executable behavior, documented intent, historical context, stale evidence, or unresolved question, followed by a qualified conclusion.

### API compatibility guardian

- **Responsibility:** Assess compatibility across ClaimHub's controller and Minimal API mix.
- **Boundaries:** Read-only. It does not approve a breaking change or update the accepted contract baseline.
- **Key evidence:** Routes, methods, authorization, contracts, validation, status and error behavior, pagination, OpenAPI, consumers, tests, and the Detect API contract drift skill.
- **Representative output:** Compatible additions and breaking changes listed separately, affected consumers, evidence, confidence, and the smallest contract test needed.

### Privacy-boundary investigator

- **Responsibility:** Follow synthetic personal and financial data through API, Admin, logs, exports, documents, audit records, errors, and authentication flows.
- **Boundaries:** Read-only. It reports locations without reproducing complete sensitive values and looks for exposure beyond what the operation and role need.
- **Key evidence:** Data models, authorization, serialization, views, logging, generated files, error handling, and the Audit generated exports for sensitive data skill.
- **Representative output:** The data flow, excessive exposure by operation or role, masked evidence locations, confidence, and focused remediation options.

### Worker side-effect investigator

- **Responsibility:** Investigate side effects produced by one Worker job.
- **Boundaries:** It does not run destructive retries against shared data or claim that one successful run proves idempotency.
- **Key evidence:** Job scheduling and selection, transactions, invoices, payments, notifications, exports, partial failures, retries, logs, and persisted state.
- **Representative output:** A repeatable reproduction plan, expected and observed side effects, behavior after partial failure or retry, and the first point at which state diverges.

## How agents and skills fit together

Here are four natural pairings:

- Premium discrepancy investigator -> Compare premium calculations.
- API compatibility guardian -> Detect API contract drift.
- Privacy-boundary investigator -> Audit generated exports for sensitive data.
- Claim lifecycle integrity auditor -> Reproduce a claim lifecycle.

The agent owns the bounded investigation, the skill supplies the reusable procedure, and the script performs the deterministic work. That leaves the agent to decide what the evidence means instead of reinventing the evidence-gathering mechanism on every run.

## Sample prompts

These prompts are a starting point for the tasks you'll run into during the workshop. The full version gives the agent more direction and asks for evidence you can check. When you don't need that much detail, start with the simpler alternative. You can always add detail if the result leaves an important question unanswered.

### Understand ClaimHub from a functional perspective

Start here when you need to understand what ClaimHub does before deciding where to make a change. The code is the primary evidence. Comparing it with the documentation may also uncover rules that have drifted.

```text
Build a functional picture of ClaimHub without editing any files. Identify the user roles, their main workflows, and the business rules that govern those workflows. Treat the running code, tests, and configuration as primary evidence. Compare that evidence with the maintained and legacy documentation, and call out contradictions or rules that the code does not support.

Focus on [WORKFLOW OR BUSINESS AREA], but inspect adjacent behavior when it changes the conclusion. For each role, workflow, or rule, cite the relevant repository-relative file path and symbol or line range. Distinguish confirmed behavior from inference, and finish with the most important open questions and a short list of realistic checks I can perform in the application.
```

**Simpler alternative:**

```text
Explain how [WORKFLOW OR BUSINESS AREA] works in ClaimHub from a functional perspective. Do not edit files. Use the code as primary evidence, compare it with the documentation, call out contradictions, and cite the repository-relative files and symbols that support your answer.
```

### Understand the repository technically

The folder structure doesn't tell you everything about how the application fits together. Use this prompt to examine both the project relationships and the path a request takes through the hosts at runtime.

```text
Map the ClaimHub repository technically without editing any files. Explain each project's responsibility and dependencies, then trace one representative request through [ENTRY POINT OR FEATURE] to storage and back. Include the shared storage model, host startup behavior, important entry points, and configuration sources.

Check the solution file against the projects and test projects that exist on disk. Call out projects or tests that are missing from the solution, along with any consequence for normal build or test commands. Cite repository-relative paths and symbols for each important claim. Finish with the exact PowerShell commands I can run from the ClaimHub root to verify the project graph, build, tests, host startup, and representative request path. Separate commands you ran successfully from commands you only recommend.
```

**Simpler alternative:**

```text
Explain the ClaimHub repository structure, project dependencies, request path, storage, and host startup. Do not edit files. Identify important entry points and configuration, check for projects or tests missing from the solution, cite your evidence, and give me the PowerShell commands that verify your findings.
```

### Research a backlog item before changing code

Use this before you implement an item, especially when the obvious place to change may not be the real source of the behavior. You want a plan tied directly to the acceptance criteria, rather than a generic list of development steps.

```text
Research backlog item [ITEM-ID] without editing any files. Read the complete item, including its acceptance criteria and dependencies. Then inspect the minimum necessary code, tests, configuration, documentation, and relevant Git history to establish the current behavior and identify the likely root cause or change area.

Report:
- the current behavior, with repository-relative evidence;
- the likely root cause or implementation area and affected components;
- risks, edge cases, dependencies, and unanswered questions;
- an implementation plan mapped to each acceptance criterion; and
- a validation plan with focused tests, build commands, and one realistic behavior check.

Distinguish verified facts from assumptions. If the evidence does not support a conclusion, say what remains unknown and name the smallest next check that would resolve it.
```

**Simpler alternative:**

```text
Investigate backlog item [ITEM-ID] without making changes. Read its criteria and dependencies, inspect the smallest relevant set of code, tests, configuration, documentation, and history, then explain the current behavior, likely change area, risks, open questions, and an implementation and validation plan mapped to the criteria.
```

### Challenge an investigation

An investigation can sound convincing while settling on one explanation too quickly. Use this as a second pass to challenge that explanation with repository evidence, rather than asking for private reasoning.

```text
Challenge this investigation conclusion: [CONCLUSION].

Do not edit files and do not ask for or expose hidden reasoning. Inspect the relevant repository evidence and look for unsupported assumptions, plausible alternative explanations, contradictions, and edge cases that could change the conclusion.

Return an evidence-based table with these columns: Assumption or conclusion, Supporting evidence, Contradicting evidence or alternative, Confidence, Next check. Cite repository-relative paths and symbols or line ranges. After the table, state whether the original conclusion still holds, needs qualification, or should be rejected, and name the smallest decisive check to run next.
```

**Simpler alternative:**

```text
Challenge the conclusion that [CONCLUSION]. Do not edit files. Find unsupported assumptions, alternatives, contradictions, and edge cases, cite repository evidence, and summarize each finding with its confidence and next check. Do not provide hidden reasoning.
```

### Decide between competing approaches

Use this when both options look reasonable and the better choice depends on how ClaimHub already works. The agent inspects the repository before it recommends either approach.

```text
Compare [APPROACH A] and [APPROACH B] for [FEATURE OR PROBLEM]. Do not implement either approach. First inspect the relevant ClaimHub code, tests, configuration, and established patterns.

Evaluate both approaches against these criteria: [DECISION CRITERIA]. Also cover maintainability, security, testability, compatibility with current behavior, and the cost or risk of migration. Cite repository-relative evidence for claims about the current system.

Recommend one approach, explain the trade-offs, and identify any condition under which the other approach would be preferable. Finish with the evidence or experiment that could change your recommendation and the smallest validation step that would provide it.
```

**Simpler alternative:**

```text
Inspect the relevant ClaimHub code, then compare [APPROACH A] and [APPROACH B] for [FEATURE OR PROBLEM]. Do not implement anything. Evaluate fit with current patterns, maintainability, security, testability, and existing behavior, then recommend one option and say what evidence could change that recommendation.
```

### Implement a feature

Use this after you've investigated the item and its boundaries are reasonably clear. It keeps the implementation focused and asks the agent to show that the change works.

```text
Implement [FEATURE] for backlog item [ITEM-ID]. Read the complete item and acceptance criteria, then inspect the relevant implementation, tests, configuration, documentation, and repository conventions before editing.

Make the smallest focused change that satisfies the criteria. Preserve existing public contracts and unrelated behavior unless the item explicitly requires a change. Update directly related documentation when behavior or usage changes. Add or update tests for the expected behavior, important boundaries, failures, and the regression this item addresses.

Validate the result by running the focused tests, the relevant build, and one realistic behavior check. Do not hide failed checks or broaden the change merely to make a test pass. When finished, summarize the files changed, key decisions and trade-offs, acceptance criteria satisfied, and exact validation commands with their results. Note any criterion or behavior you could not verify.
```

**Simpler alternative:**

```text
Implement [FEATURE] for [ITEM-ID]. Inspect the relevant code and tests first, keep the change focused, preserve existing contracts and unrelated behavior, update related documentation, and cover important behavior and edge cases with tests. Run focused tests, the build, and a realistic behavior check, then summarize the changed files, decisions, and validation evidence.
```

### Create or improve tests

Use this when you want stronger evidence about behavior without tying the tests to the current implementation. The prompt first asks the agent to inspect the existing test conventions, because those still matter.

```text
Create or improve tests for [FILE OR SYMBOL] and the behavior [BEHAVIOR]. Inspect the implementation and nearby test conventions before editing.

Cover the expected behavior, meaningful boundaries, failure paths, and the regression described by [ITEM-ID OR BUG]. Prefer observable inputs, outputs, state changes, and side effects over implementation details. Do not weaken assertions, delete useful coverage, or change production behavior solely to make a test easier to write.

Run the smallest relevant test set first, followed by any broader affected test project. Report the exact commands and results, the behaviors now covered, and any important gap that remains. If coverage tooling is already available in the repository, report the relevant coverage result; do not add a new coverage dependency unless I ask for it.
```

**Simpler alternative:**

```text
Improve the tests for [FILE OR SYMBOL]. Inspect the implementation and existing test style first, then cover normal behavior, boundaries, failures, and the relevant regression through observable behavior. Do not change production behavior only for the test. Run the relevant tests and report the commands, results, and remaining coverage gaps.
```

### Review a completed change

Run this after the implementation and validation are complete. The agent reviews the whole diff and enough of the surrounding code to find consequences that may not be visible in the changed lines.

```text
Review the complete current diff for [ITEM-ID OR FEATURE] without editing files. Read the backlog item and acceptance criteria, inspect all changed files plus the surrounding implementation, tests, configuration, and documentation needed to understand the change.

Look for correctness problems, regressions, missed edge cases, unsupported assumptions, incomplete acceptance criteria, and weak or missing tests. Report only actionable findings, ordered by severity. For each finding, include severity, repository-relative file and line, the concrete impact, the evidence, and a minimal remediation. Do not report style preferences unless they create a real maintenance or correctness problem.

If there are no findings, say so explicitly. In either case, list the validation evidence you checked and any gaps that prevent full confidence.
```

**Simpler alternative:**

```text
Review the full diff for [ITEM-ID OR FEATURE] against its acceptance criteria. Do not edit files. Inspect the surrounding code and tests, then report actionable correctness, regression, edge-case, assumption, or test findings by severity with file, line, impact, and remediation. If there are no findings, say so and note any validation gaps.
```

### Perform a security review

Use this to keep the review focused on vulnerabilities that someone could exploit. Tracing data through the application gives you more useful evidence here than a list of general security advice.

```text
Perform a read-only security review of [FEATURE, ENDPOINT, OR FILE OR SYMBOL]. Trace untrusted input through authentication, authorization, validation, persistence, logging, file access, and external calls wherever those stages apply.

Look for exploitable vulnerabilities rather than style issues. For each finding, report severity, confidence, repository-relative location, attack scenario, concrete impact, supporting evidence, and the smallest effective remediation. Note tests, configuration, or framework behavior that mitigates the risk. Distinguish a confirmed vulnerability from a defense-in-depth improvement or an unanswered question.

Do not edit files, run destructive checks, or include synthetic credential values or personal data in the report. Finish with the safe, focused commands or tests that would validate the most important findings.
```

**Simpler alternative:**

```text
Perform a read-only security review of [FEATURE, ENDPOINT, OR FILE OR SYMBOL]. Trace untrusted input through authentication, authorization, validation, persistence, logging, file access, and external calls. Report exploitable vulnerabilities with severity, confidence, location, attack scenario, impact, evidence, and minimal remediation. Do not expose synthetic credentials or personal data.
```

### Prepare a hand-off to a new session

Use this before the context becomes crowded, or when someone else will continue the item. The hand-off needs to be ready to paste, with the verified state and a clear stopping point instead of a long narrative.

```text
Stop implementation and prepare a ready-to-paste prompt for a new session. Inspect the current Git diff and status, backlog item [ITEM-ID], changed files, and the most recent validation evidence.

Keep the hand-off at 300 words or fewer. Include:
- the goal and relevant acceptance criteria;
- work completed and the current observable state;
- repository-relative paths for changed or important files;
- key decisions and constraints;
- unresolved questions, risks, or failures;
- exact commands already run and their results; and
- the next action, including the precise stop point.

Clearly distinguish verified facts from assumptions and proposed next steps. Do not continue editing, rerun expensive checks without need, or include machine-specific absolute paths.
```

**Simpler alternative:**

```text
Stop implementation and create a ready-to-paste hand-off prompt for [ITEM-ID] in 300 words or fewer. Inspect the diff, status, item, and recent validation. Include completed work, current state, repo-relative files, decisions, open questions, commands and results, and the exact next action. Separate verified facts from assumptions.
```

### Create repository or scoped instructions

Use this when a rule needs to remain available after the current task. First decide whether it belongs in the short always-on file or in a scoped file that loads only for matching code.

```text
Propose repository instructions for [RULE OR WORKFLOW]. Inspect existing repository customizations, maintained documentation, representative files, and current conventions before proposing changes.

Use `.github/copilot-instructions.md` only for short, stable, always-on rules. Use a file under `.github/instructions` with an accurate `applyTo` pattern for rules that apply only to a scope such as tests, API code, EF Core, or Razor Pages. Base each rule on repository evidence, reference maintained documentation instead of duplicating it, and exclude temporary task details or one-off decisions.

Show me the proposed file choice, content, supporting evidence, and validation plan before editing. After approval, make only the agreed change. Verify the scoped behavior with at least one representative path that should match and one that should not, then report the evidence and any client-dependent limitation.
```

**Simpler alternative:**

```text
Inspect the existing customizations and propose instructions for [RULE OR WORKFLOW]. Put only short always-on rules in `.github/copilot-instructions.md`; use `.github/instructions` with an accurate `applyTo` for scoped rules. Base the proposal on repository evidence, reference maintained docs, avoid task-specific details, and show the proposal before editing. Verify one matching and one non-matching path.
```

### Create a reusable skill

Create a skill for a workflow you expect to repeat, particularly when it needs an ordered procedure and executable checks. Keep its scope clear enough that another session can tell when the skill applies and when it doesn't.

```text
Create a reusable repository skill at `.github/skills/[SKILL-NAME]` for [REPEATABLE WORKFLOW]. Before editing, inspect the current workflow, relevant scripts, repository conventions, existing skills, and one representative task.

Define when the skill should and should not be used, required inputs, an ordered procedure, expected output, failure handling, and executable validation. Keep `SKILL.md` focused. Add supporting scripts, references, or examples only when they make the workflow more reliable or keep `SKILL.md` concise.

Do not include secrets, synthetic credential values, machine-specific paths, temporary task details, or one-off conclusions. Validate the completed skill on [BOUNDED TASK], report the actions it caused, exact checks and results, and any ambiguity or failure you found.
```

**Simpler alternative:**

```text
Create `.github/skills/[SKILL-NAME]` for [REPEATABLE WORKFLOW]. Inspect the workflow, scripts, conventions, and existing skills first. Define when to use it, when not to, inputs, ordered steps, output, failures, and executable checks. Keep `SKILL.md` focused, avoid secrets and machine-specific or one-off details, and validate it on [BOUNDED TASK].
```

### Create a focused custom agent

Use a custom agent when one clear responsibility needs its own boundaries and tool set. Test it with a bounded scenario as well, because the description alone doesn't enforce what the agent can do.

```text
Create a focused repository custom agent under `.github/agents` for [RESPONSIBILITY]. Inspect existing agents, repository instructions, the relevant workflow, and client conventions before editing.

Give the agent one clear responsibility and a description that makes selection unambiguous. Grant only the minimum tools needed. State its operating boundaries, required evidence format, and these working rules: inspect before editing, preserve unrelated behavior, surface failures and uncertainty, and run the relevant checks before reporting success.

Do not treat the agent description as a security boundary; tool permissions, repository policy, review, and executable checks still apply. Test the agent on [BOUNDED SCENARIO], then review and report both the actions it took and the output it produced, including failed checks or behavior outside its intended scope.
```

**Simpler alternative:**

```text
Create a custom agent under `.github/agents` for [RESPONSIBILITY]. Inspect existing customizations and the workflow first. Give it one responsibility, a clear selection description, minimum tools, explicit boundaries, and an evidence format. Require inspection before edits, preserved behavior, surfaced failures, and relevant checks. Test it on [BOUNDED SCENARIO] and review its actions as well as its output.
```
