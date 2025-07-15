# Architecture

_Last major update: 22 August 2023_

PolicyAdmin is a three-tier .NET application backed by SQL Server. ClaimHub is being built inside that estate, not as a separate product.

```text
Broker portal / staff browser
             |
       ClaimHub.Web
             |
       ClaimHub WCF service
          /       \
 PolicyAdmin DB   Document share
             |
      overnight batch
```

`ClaimHub.Web` contains MVC controllers, Razor views and application services. It calls the ClaimHub WCF service for claims and uses the PolicyAdmin repositories for customer and policy reads. The WCF service owns claim transitions and writes to the `ClaimHub` schema in the shared SQL Server database.

The document share is mounted as `\\claims-fs\claimhub`. Only document identifiers are stored in SQL Server. We looked at blob storage, but postponed it because nobody had settled retention or cost ownership.

## Layering rules

Web projects must not query SQL directly. Data access is through repositories and the unit of work. Services return DTOs mapped with AutoMapper. JSON integrations use Newtonsoft.Json because PolicyAdmin consumers rely on its enum and date formatting.

The overnight Windows service handles invoices, dunning, renewals and exports. It runs on one server, so jobs assume there is no concurrent execution.

## Direction for CH2

CH2 is the proposed extraction of claims from PolicyAdmin. The current proposal is:

- a standalone HTTP API;
- a separate handler portal;
- local SQLite for developer machines, while production remains on SQL Server;
- incremental migration, not a big-bang cutover.

We have not decided between controllers and endpoint modules. Until CH2 starts, follow the PolicyAdmin conventions above.

_Tom De Smet_
