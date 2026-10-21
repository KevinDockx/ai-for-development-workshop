# Operations runbook

_Service Operations copy, reviewed 11 January 2025_

## Components

ClaimHub has a web front end, an API/WCF compatibility service and a batch process. All three use the production SQL Server cluster. Claim attachments are on the claims file server. The application pool and the batch service run under separate managed accounts.

CH2 development environments may use SQLite. This must never be promoted to production, and the production connection string must not be committed to the repository.

## Scheduled work

| Job | Time | Notes |
|---|---:|---|
| Invoice generation | 00:30 | One invoice per instalment |
| Payment import | 01:15 | File supplied by payment provider |
| Dunning | 02:00 | Reminder 15 days overdue; suspend after 45 days |
| Renewal | 03:00 | Creates next annual term |
| Claims SLA | 06:15 | Escalates unassessed claims |
| Customer export | 23:15 | Restricted folder |

Check the run table before rerunning invoice generation. The batch was designed as single-instance, and a second execution can create duplicate invoices.

Refunds are represented as negative invoices, not direct edits to payments. Claim payouts and refunds use the payment gateway. Keep the gateway reference with the transaction so a retry can be reconciled.

## Dates

Business dates are Belgian dates. Service timestamps are UTC in CH2, although old PolicyAdmin tables contain local server timestamps. The claims SLA excludes weekends. We have not loaded the holiday calendar, so public holidays currently count as working days.

## Documents

Documents are written beneath the configured storage root. Back up the database and document tree as one recovery unit. The folder layout is an implementation detail and must not be exposed to users.

## Incident checks

1. Check API and web health.
2. Check database connectivity and available disk space.
3. Confirm whether the overnight job completed.
4. Compare the job run identifier before starting anything manually.
5. Record the action in the incident timeline.

Escalate claim-state or payout inconsistencies to Claims Operations. Do not try to repair those records with an ad-hoc delete.

_Sofie Vermeulen_
