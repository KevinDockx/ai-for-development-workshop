# Claim reopening — version 0

**Status:** Delivered in Q3  
**Programme:** PolicyAdmin claims improvements  
**Owner:** Claims Operations  
**Last updated:** 2 October 2023

## Purpose

Handlers sometimes receive an invoice, police report or medical certificate after a claim has been completed. The reopening function lets them reuse the existing file instead of registering a duplicate claim.

## Rules

- A `Closed` or `PaidOut` claim can be reopened for 180 calendar days after its last status change.
- Reopening does not require new evidence. A handler may reopen to correct an assessment note or payment reference.
- Rejected claims cannot be reopened; register a new claim if the rejection is challenged.
- The original claims handler may reopen their own claim. A senior handler is not required.
- Reopening moves the claim directly to `Approved` when it has previously been paid, otherwise to `UnderAssessment`.
- The previous deductible and coverage usage are retained.
- The SLA clock continues from the original report date.
- The action and free-text reason are added to the claim history.

## User interface

The Closed Claims page shows **Reopen** when the file is within the 180-day window. After confirmation, the handler returns to the claim detail page. The broker and policyholder do not receive an automatic notification.

## Service change

The WCF operation is:

```text
ReopenClaim(claimId, handlerId, reason)
```

It returns the updated claim status. After a successful call, consumers should refresh the complete claim.

## Rollout note

Delivered with the Q3 PolicyAdmin package. The old terminal client does not show the new history entry. This does not block use by the claims team.

_Prepared by Nadia Peeters and Bram Jacobs_
