# ClaimHub workshop backlog

During the day, you will improve both ClaimHub and the repository context that helps AI work safely in it. Work in your own copy, but compare findings and discuss choices with a neighbour. When you take an item, change its front matter from `open` to `in-progress`. Use `blocked` when a real dependency stops you, and move it to `done` only when its acceptance criteria are met.

## Start here

1. ORI-01
2. ORI-04
3. AIR-01
4. AIR-02

## Suggested path through the day

Start by understanding the application and checking that you can run it. Then improve the repository context before taking on larger code changes, because the instructions and domain documentation you add will help with every later item. Pick a bug, security finding, or debt item next and give your agent a way to verify its work. Once you have repeated an approach, turn it into an agent, skill, or MCP exercise. Features and the larger cross-cutting work fit best after that foundation is in place.

Taking an item means that you own it and have changed its status to `in-progress`. Move it to `done` when every acceptance criterion is checked, the relevant tests and quality checks pass, and any affected documentation, ADRs, or glossary entries are current. A first AI-generated change that compiles is only a step along the way.

## How to read an item

- **Type** tells you whether the work is orientation, AI readiness, an agent, a skill, MCP, security, a bug, technical debt, a feature story, or testing.
- **Priority** is the business urgency: `critical`, `high`, `medium`, or `low`.
- **Size** is a workshop estimate: `S`, `M`, or `L`.
- **AI practice** names the technique worth practising. An empty list means the item does not prescribe one.
- **Status** has exactly four values: `open`, `in-progress`, `blocked`, and `done`.

## Orientation

- [ORI-01 - Create an architecture overview](items/ORI-01-create-architecture-overview.md)
- [ORI-02 - Trace the claim lifecycle](items/ORI-02-trace-claim-lifecycle.md)
- [ORI-03 - Find every premium calculation](items/ORI-03-find-premium-calculations.md)
- [ORI-04 - Run ClaimHub and verify its README](items/ORI-04-run-claimhub-and-verify-readme.md)
- [ORI-05 - Mine the history for decisions](items/ORI-05-mine-history-for-decisions.md)

## AI readiness

- [AIR-01 - Add repository-wide AI instructions](items/AIR-01-add-repository-ai-instructions.md)
- [AIR-02 - Restructure the repository for AI-assisted work](items/AIR-02-restructure-repository-for-ai-work.md)
- [AIR-03 - Turn meeting notes into ADRs](items/AIR-03-turn-meeting-notes-into-adrs.md)
- [AIR-04 - Build the ClaimHub glossary](items/AIR-04-build-claimhub-glossary.md)
- [AIR-05 - Extract business invariants](items/AIR-05-extract-business-invariants.md)
- [AIR-06 - Replace the stale claim reopening spec](items/AIR-06-replace-claim-reopening-spec.md)
- [AIR-07 - Document and script repository commands](items/AIR-07-document-and-script-commands.md)
- [AIR-08 - Add scoped AI instructions](items/AIR-08-add-scoped-ai-instructions.md)
- [AIR-09 - Clean up code-local context](items/AIR-09-clean-code-local-context.md)
- [AIR-10 - Rewrite README and CONTRIBUTING](items/AIR-10-rewrite-readme-and-contributing.md)
- [AIR-11 - Add automated coding feedback](items/AIR-11-add-automated-coding-feedback.md)
- [AIR-12 - Review repository context quality](items/AIR-12-review-context-quality.md)

## Agents

- [AGT-01 - Audit repository health with subagents](items/AGT-01-audit-with-subagents.md)
- [AGT-02 - Create a read-only security reviewer](items/AGT-02-create-security-reviewer.md)
- [AGT-03 - Create a characterization test writer](items/AGT-03-create-test-writer.md)
- [AGT-04 - Create or adapt an ADR writer](items/AGT-04-create-adr-writer.md)
- [AGT-05 - Plan and hand off a feature](items/AGT-05-plan-and-hand-off-feature.md)
- [AGT-06 - Coordinate cross-cutting work](items/AGT-06-coordinate-cross-cutting-work.md)

## Skills

- [SKL-01 - Review and adapt an existing skill](items/SKL-01-review-and-adapt-skill.md)
- [SKL-02 - Create a Minimal API endpoint skill](items/SKL-02-create-minimal-api-skill.md)
- [SKL-03 - Create an EF Core migration skill](items/SKL-03-create-ef-migration-skill.md)
- [SKL-04 - Create an audit checklist skill](items/SKL-04-create-audit-checklist-skill.md)
- [SKL-05 - Create a backlog item skill](items/SKL-05-create-backlog-item-skill.md)
- [SKL-06 - Choose a prompt file or skill](items/SKL-06-choose-prompt-or-skill.md)

## MCP

- [MCP-01 - Verify a .NET API with Microsoft Learn MCP](items/MCP-01-verify-dotnet-api-with-learn.md)
- [MCP-02 - Build a backlog MCP server](items/MCP-02-build-backlog-mcp-server.md)
- [MCP-03 - Investigate SQLite through read-only MCP](items/MCP-03-investigate-sqlite-read-only.md)
- [MCP-04 - Expose legacy tickets through MCP](items/MCP-04-expose-legacy-tickets.md)

## Security

- [SEC-01 - Crafted claim search exposes unintended data](items/SEC-01-crafted-search-exposes-data.md)
- [SEC-02 - Users can view policies outside their scope](items/SEC-02-policy-scope-bypass.md)
- [SEC-03 - Document downloads cross trust boundaries](items/SEC-03-document-download-boundaries.md)
- [SEC-04 - Secrets are committed to the repository](items/SEC-04-committed-secrets.md)
- [SEC-05 - Password storage fails company policy](items/SEC-05-password-storage-policy.md)
- [SEC-06 - Profile updates allow role changes](items/SEC-06-profile-role-change.md)
- [SEC-07 - Logs and exports expose personal data](items/SEC-07-pii-in-logs-and-exports.md)
- [SEC-08 - Refund and token authorization can be bypassed](items/SEC-08-refund-and-token-bypass.md)
- [SEC-09 - Admin portal accepts unsafe navigation and content](items/SEC-09-admin-input-and-navigation.md)
- [SEC-10 - Audit dependencies and serialization](items/SEC-10-dependency-serialization-audit.md)

## Bugs

- [BUG-01 - Installments do not add up](items/BUG-01-installments-do-not-add-up.md)
- [BUG-02 - Cancellation refunds are wrong](items/BUG-02-wrong-cancellation-refund.md)
- [BUG-03 - Policy dates shift around boundaries](items/BUG-03-policy-date-boundaries.md)
- [BUG-04 - Invalid claims can be approved](items/BUG-04-invalid-claim-approval.md)
- [BUG-05 - Discounts can create a negative premium](items/BUG-05-negative-premium.md)
- [BUG-06 - Claims pagination skips page one](items/BUG-06-pagination-skips-page-one.md)
- [BUG-07 - Admin claims list is slow](items/BUG-07-slow-admin-claims-list.md)
- [BUG-08 - Billing creates duplicate invoices](items/BUG-08-duplicate-invoices.md)
- [BUG-09 - Claims are escalated too early](items/BUG-09-early-claim-escalation.md)
- [BUG-10 - Admin and API premiums disagree](items/BUG-10-premium-disagreement.md)
- [BUG-11 - A payout can run twice](items/BUG-11-duplicate-payout.md)
- [BUG-12 - The worker stops overnight](items/BUG-12-worker-stops-overnight.md)

## Technical debt

- [DEB-01 - Standardize Billing data access](items/DEB-01-standardize-billing-data-access.md)
- [DEB-02 - Migrate policy endpoints](items/DEB-02-migrate-policy-endpoints.md)
- [DEB-03 - Replace AutoMapper deliberately](items/DEB-03-replace-automapper.md)
- [DEB-04 - Standardize validation](items/DEB-04-standardize-validation.md)
- [DEB-05 - Standardize error responses](items/DEB-05-standardize-error-responses.md)
- [DEB-06 - Split the policy service](items/DEB-06-split-policy-service.md)
- [DEB-07 - Enable nullable and warnings as errors](items/DEB-07-enable-nullable-and-warnings.md)
- [DEB-08 - Standardize policyholder naming](items/DEB-08-standardize-policyholder-naming.md)

## Features

- [FEA-01 - Reopen a closed claim](items/FEA-01-reopen-closed-claim.md)
- [FEA-02 - Require four-eyes approval](items/FEA-02-four-eyes-approval.md)
- [FEA-03 - Add bicycle insurance](items/FEA-03-add-bicycle-insurance.md)
- [FEA-04 - Notify policyholders of claim changes](items/FEA-04-notify-claim-status.md)
- [FEA-05 - Download a policy schedule](items/FEA-05-download-policy-schedule.md)
- [FEA-06 - Show fraud flag reasons](items/FEA-06-show-fraud-reasons.md)

## Tests

- [TST-01 - Characterize premium calculation](items/TST-01-characterize-premium-calculation.md)
- [TST-02 - Make the late-evening test deterministic](items/TST-02-fix-time-dependent-test.md)
- [TST-03 - Restore API integration tests](items/TST-03-restore-api-integration-tests.md)
- [TST-04 - Enforce architecture boundaries](items/TST-04-enforce-architecture-boundaries.md)
- [TST-05 - Add a local quality gate](items/TST-05-add-local-quality-gate.md)
