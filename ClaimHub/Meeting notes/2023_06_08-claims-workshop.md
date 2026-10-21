# Notes from claims design workshop

**Date:** 8 June 2023  
**Present:** Nadia Peeters, Tom De Smet, Bram Jacobs, Elise Van Acker, Anouk Willems

We put reporting, assessment, approval and payout on the whiteboard. Nadia wants the complete workflow readable in one place. Claims should not depend on whichever HTTP or WCF front end happens to call it.

Bram proposed MediatR for commands and queries. Tom did not want to add a package for a small dispatch mechanism and then couple every handler to its types. He will accept a small mediator in the core, with plain request and handler contracts. Keep it boring: dispatch and cancellation, with no hidden pipeline of business behaviour.

Event sourcing came up because a claim has a useful history. Nobody saw enough benefit to justify the operational cost. We still need an append-only status history and audit entries, but the application will continue to use the current claim row as its state.

Nadia confirmed the initial path: Reported, Under Assessment, Approved or Rejected, then payment and closure where applicable. Withdrawn is final. A late report is a flag, not an automatic rejection.

## Follow-up

- Bram will build the in-house mediator and two claim handlers.
- Anouk will add workflow examples around invalid transitions.
- Nadia will provide the audit fields handlers need to see.
- Event sourcing is parked. Do not reopen it without a concrete reporting or audit requirement.
