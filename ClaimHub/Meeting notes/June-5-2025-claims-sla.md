# Claims SLA calculation

**Meeting:** June 5, 2025  
**Attendees:** Nadia Peeters, Elise Van Acker, Pieter Claes, Bram Jacobs, Anouk Willems

The escalation list currently says “five days”, but handlers read that as five working days. A claim reported late on Friday should not be escalated on Wednesday morning just because the weekend was counted.

We will measure the first-assessment SLA in business days. For the first version, a business day means Monday through Friday. Belgian public holidays are ignored until we have an agreed calendar source and somebody owns its maintenance. Nadia would rather document that limitation than rely on a hand-maintained list that quietly becomes stale.

The clock starts when the report is received, not on the date of loss. At five completed business days without a first assessment, the claim appears in the senior claims handler escalation queue.

Bram said the current worker adds calendar days. The operations page has the right wording, but the implementation still needs a separate change.

## Actions

- Bram: change the SLA calculation to skip weekends.
- Anouk: add Friday and weekend examples.
- Nadia: confirm the point during day five at which escalation appears.
- Elise: open a later item for a maintained holiday calendar; it is out of scope now.
