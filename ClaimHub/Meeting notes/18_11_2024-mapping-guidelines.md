# Mapping guidelines

**Date:** 18 November 2024  
**Attendees:** Tom De Smet, Bram Jacobs, Anouk Willems

We currently have hand-written mappings in claims, direct entity returns in two older endpoints and AutoMapper profiles in policies. Reviews keep getting stuck on which approach is allowed.

Tom wants one approach. AutoMapper is already used in PolicyAdmin and handles the larger policy models without repetitive assignments. We will standardize on AutoMapper for mappings between entities, service models and contracts.

Profiles belong near the consuming application. Domain code should not take a dependency on AutoMapper. A pull request that introduces a DTO must include the profile and a mapping test when the shape is not trivial.

Bram asked whether the working claims extension methods have to be replaced immediately. We did not agree a migration ticket. New mappings should use AutoMapper, and existing mapping can move when that area changes.

## Actions

- Tom: update the contribution guide.
- Bram: add a claims profile with the next claims contract.
- Anouk: add a mapping smoke test to the API test project.
- Review this after the next AutoMapper upgrade.
