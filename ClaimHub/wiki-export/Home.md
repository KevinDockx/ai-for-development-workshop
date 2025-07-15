# Claims Platform Wiki

> Exported from the Claims Platform team wiki on 19 December 2024. Links to attachments and dashboards were not included in the export.

ClaimHub is the working name for the claims and policy servicing programme. We are replacing the claims module in PolicyAdmin in stages. Policy issuance stays in the core until that migration is complete.

## What lives where

| Area | System of record |
|---|---|
| Customer and broker | PolicyAdmin |
| Policy and coverage | the core |
| Claim and claim documents | ClaimHub |
| Invoice and payment | PolicyAdmin billing |
| Overnight extracts | Claims batch server |

The boundaries are not clean yet. ClaimHub keeps a local copy of customers and policies so handlers can carry on working while PolicyAdmin is in maintenance. If values differ, the nightly reconciliation is authoritative.

## Product shorthand

- Home covers building and/or contents. Postcode zone, insured values and construction year drive the risk.
- Motor always includes third-party liability. Omnium and glass are optional.
- Travel is annual and can be single or family, Europe or worldwide.

Older pages use “client”, “customer”, “insured” and “policyholder” for what may be the same person. In practice they are not always the same: a parent can own a travel policy covering family members.

## Pages in this export

- [Architecture](Architecture.md)
- [Claims process](Claims-process.md)
- [Premium calculation](Premium-calculation.md)
- [Policy lifecycle](Policy-lifecycle.md)
- [Operations](Operations.md)
- [Onboarding](Onboarding.md)

Questions about claim handling go to Nadia. Rating questions go to Elise or Pieter. Deployment and database matters go to Sofie.

_Page owner: Elise Van Acker_
