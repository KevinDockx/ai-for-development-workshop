# ClaimHub

ClaimHub replaces the claims screens in PolicyAdmin. It includes the customer and policy administration site, claims handling, billing jobs and the integration service used by the broker portal.

The solution is still called ClaimHub even though the web project is `ClaimHub.Web`.

## Before you start

You need:

- Visual Studio 2022 with the ASP.NET and web development workload
- .NET 7 SDK
- SQL Server 2019 Developer Edition or a shared development instance
- SQL Server Management Studio
- PowerShell 5.1

Ask Operations for a copy of `claimhub-dev.bak`, then restore it as `ClaimHub_Dev`. Put your SQL Server credentials in `ClaimHub.Web\appsettings.Development.json`. Use Windows authentication if you can.

## Running locally

From a Developer PowerShell prompt:

```powershell
.\run.bat
```

The script restores packages, applies the database scripts and starts both sites:

- ClaimHub Web: `https://localhost:44318`
- ClaimHub API: `https://localhost:44319`

Swagger is available at `https://localhost:44319/swagger`. If the browser reports a certificate warning, trust the development certificate and restart Visual Studio.

You can also set `ClaimHub.Web` and `ClaimHub.Api` as multiple startup projects. Only start the billing console when you need to test the overnight run. It creates invoices in the development database straight away.

## Database updates

Database changes are maintained in `database\scripts`. Add a numbered SQL file and update `database\scripts\manifest.txt`. Entity Framework migrations are not used in this repository.

Never point a local checkout at the production database. Production configuration is maintained by Operations and is not stored here.

## Tests

Run the `ClaimHub.Tests` project from Test Explorer. The integration tests expect SQL Server and must not run in parallel.

## Useful background

The exported team wiki is under `wiki-export`. Start with `Home.md`. Some pages still call the system PolicyAdmin or “the core”, so watch out for that. Meeting decisions are in `Meeting notes`.

For access to a development account or test policy, ask the Claims Platform team. Use generated customer data only.

_Last checked by Bram, 6 October 2023._
