# Policy lifecycle

_Product note, last edited 9 September 2024_

A quote becomes active when cover is accepted. From `Active`, a policy can be cancelled, expire at the end of its term or be renewed into a following term.

```text
Quoted -> Active -> Cancelled
                  -> Expired
                  -> Renewed
```

Suspended is a billing hold used by PolicyAdmin. It does not end cover on its own, so do not treat it as a cancellation.

## Coverage period

Coverage uses a start-inclusive, end-exclusive period: `[StartDate, EndDate)`. A loss on the end date belongs to the next term, if one exists. Dates are Belgian calendar dates; system events are recorded in UTC.

A normal term is one year. For a policy starting 29 February, the term ends 28 February in the following year and renewal starts on that 28 February. The old core shows 27 February as “last covered day”. That is the same boundary expressed differently.

## Cancellation

Refund the unused premium using the remaining days divided by the actual number of days in that policy term. Do not divide a leap-year term by a fixed 365.

No cancellation refund is due after a claim has been paid in the current term. Claims that are only reported or rejected do not block the refund.

## Endorsements

Address, vehicle, insured person, coverage, value and payment-frequency changes are endorsements. Recalculate the premium for the days remaining in the term. Do not rerate days before the effective date.

The policyholder owns the contract, but an insured person may be somebody else. Brokers act for Scheldt Assurance and can only work with policies in their own portfolio.

_Elise Van Acker_
