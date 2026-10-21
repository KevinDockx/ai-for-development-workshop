# Claims process

_Claims Operations working page — Nadia Peeters, updated 4 February 2025_

## Standard flow

1. A claim is reported against a policy and coverage.
2. A handler starts the assessment.
3. The handler records the assessed amount and either approves or rejects.
4. An approved claim is paid out and closed.
5. A rejected claim is closed after the decision letter is sent.

The statuses used by ClaimHub are `Reported`, `UnderAssessment`, `Approved`, `Rejected`, `PaidOut`, `Closed` and `Withdrawn`. A claim can be withdrawn before it reaches a final status. Closed and withdrawn files are final. The Q3 reopening enhancement is described separately in the specs folder.

The loss date must be within the policy coverage dates, and the policy must have been active on that date. The end date itself is not covered. Reports arriving more than 30 days after the loss are marked late for the handler; they are not rejected automatically.

## Assessment and payment

The payable amount is the lower of:

- assessed amount less the coverage deductible; and
- the unused coverage limit for the current policy term.

The result cannot be below zero. A deductible applies per claim, while a coverage limit applies across the policy term.

An approval over EUR 10,000 must be checked by a senior claims handler before payment. Until the second-approval screen is available, the handler adds the senior colleague’s initials to the assessment note and moves the claim on.

## Fraud indicators

The following indicators are shown for review:

- more than two claims in the previous twelve months;
- loss within fourteen days after cover started;
- assessed amount above eighty percent of the coverage limit.

These indicators are there for the handler to review. They are not, by themselves, a reason to reject the claim.

## Service level

First assessment is due five working days after reporting. We count Monday to Friday. Public holidays are not in the calendar yet. Overdue work goes to the senior-handler queue.

## Records

Do not delete a claim to correct a mistake. Withdraw it, add a note and keep the history. The documents and status changes are part of the claim file.
