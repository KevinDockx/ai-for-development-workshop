# Joining the ClaimHub team

Welcome. These are the things people usually end up discovering during their first week.

## Access

Request membership of the Claims Platform group, SQL access to `DEV-SQL-14`, the PolicyAdmin test environment and the shared document folder. Production access is not needed for development.

Do not copy a real claim into a local database. The test-data pack uses invented names, registration numbers and bank accounts. A registration number is still personal data, even if the screen makes it look like an ordinary identifier.

## Get the application running

Clone the repository, restore `claimhub-dev.bak`, update the `ClaimHub.Web` connection string and run `run.bat`. The web site should open on port 44318. See the repository README if certificates fail.

The usual startup set is:

1. `ClaimHub.Web`
2. `ClaimHub.Api`
3. `ClaimHub.Billing` only when required

The team has a SQLite experiment for CH2, but it is not the supported path and its data is wiped frequently.

## First reading

Before taking a ticket, read Claims process and Policy lifecycle. Then follow one claim from its policy through assessment and payment. Some code says `Customer`, some says `Client`, and newer code says `PolicyHolder`. Ask before doing a broad rename because integration payloads use all three.

## Team habits

Stand-up is at 09:20. Claims triage is Tuesday and Thursday. Nadia decides operational questions; Elise owns product interpretation; Tom handles architecture calls. Sofie should be involved before changing jobs, storage or configuration.

Small fixes still need a ticket and review. For money, dates or a status transition, add examples to the pull request instead of writing “covered by unit tests”.

_Anouk Willems, March 2024_
