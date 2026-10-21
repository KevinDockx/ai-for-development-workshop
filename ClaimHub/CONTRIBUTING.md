# Contributing to ClaimHub

These are the conventions we use to keep ClaimHub in line with PolicyAdmin and the other Scheldt Assurance applications. They apply to new work as well as maintenance.

## General

- Keep pull requests below 500 changed lines where possible.
- Include the ticket key in the branch and commit message.
- Do not mix formatting changes with a functional change.
- Add or update a unit test for every business rule.
- Use `DateTime.Now` for business dates and timestamps. Our servers run on Belgian time, so converting to UTC only makes investigation harder.

## API conventions

All HTTP endpoints must be implemented in MVC controllers. Do not add Minimal APIs: we need controller filters and a predictable folder structure.

Keep controllers thin. They validate the request, call a service, map the result and return the correct status code. Route names use PascalCase after `/api`, for example `/api/Policies`.

Use Newtonsoft.Json everywhere, including in new projects. Configure enums with `StringEnumConverter`. Do not mix `System.Text.Json` into the same application. The resulting payload differences are difficult for consumers to diagnose.

## Data access

All data access goes through repositories. A service must not inject `ClaimHubDbContext`, execute raw SQL or introduce a query object. Add a repository method even when the query is used only once.

Call `IUnitOfWork.SaveChanges()` once at the end of the service operation. Repository entities must not leave the service layer.

## Mapping

Use AutoMapper for all mapping between entities, service models and API models. Manual projection and mapping extension methods are not accepted. Mapping belongs in a `Profile` in the web project.

## Time and money

Use `DateTime.Now` and `DateTime.Today`; do not introduce a clock abstraction for normal application code. Policy dates are stored without a time component.

Keep the existing numeric type in the area you are changing. Round monetary results to two decimals before returning them to the web layer.

## Pull-request checklist

- [ ] The solution builds without new warnings.
- [ ] The SQL Server database scripts run on a clean database.
- [ ] New endpoints are in a controller.
- [ ] Newtonsoft.Json is used for JSON.
- [ ] Data access is behind a repository.
- [ ] AutoMapper profiles cover all new models.
- [ ] Business dates use local server time.

_Architecture group, November 2023._
