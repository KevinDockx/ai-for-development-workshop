# ClaimHub technical catch-up — 16 February 2023

**Attendees:** Tom De Smet, Sofie Vermeulen, Bram Jacobs, Anouk Willems  
**Topic:** Developer database for the ClaimHub extraction

Sofie said that getting every developer a SQL Server database has become the slowest part of onboarding. LocalDB helped on Windows, but not with Bram’s container experiment. What the team needs is a local store that can be recreated without an Operations request.

Bram showed the EF Core spike on SQLite. The claim and policy tables used by the spike worked. Date types and concurrent writes are different from SQL Server, so this is a development choice, not a production database proposal.

Tom was fine with SQLite for local ClaimHub work, provided nobody mistakes provider-specific behaviour for production behaviour. Production stays on SQL Server. Its configuration belongs in the deployment system and should not be copied into the repository.

Anouk asked for at least one SQL Server test before any release involving queries or migrations. We did not settle where that test will run.

## Actions

- Bram: add a reproducible SQLite database initializer to the CH2 branch.
- Sofie: document the production-configuration boundary.
- Anouk: list the database behaviours that need a SQL Server release check.
- Tom: revisit if SQLite starts shaping the domain model.
