# Mapping follow-up — 17 April 2025

**Present:** Tom De Smet, Bram Jacobs, Anouk Willems, Elise Van Acker

AutoMapper’s licensing change alters the assumptions behind the November decision. Tom does not want another dependency review whenever somebody adds a small API model, and the team still cannot confirm the eventual production cost.

For now, stop adding AutoMapper. Existing profiles continue to run. Removing them all would create risk without adding product value. New code should use explicit manual mapping, preferably a small extension method or projection beside the feature. Do not return an EF entity just to avoid writing a mapper.

Elise asked whether this blocks policy work because policies already rely on profiles. It does not. The policy area can keep its current profiles until it changes for another reason. This supersedes the “AutoMapper for all mapping” direction from November.

Anouk will treat mixed styles as expected during the transition, but a single feature should not use two styles without a reason.

## Actions

- Tom: amend the architecture guidance and mark the November decision superseded.
- Bram: keep new claims response mapping explicit.
- Anouk: remove “must use AutoMapper” from the review checklist.
- Tom: revisit replacement/removal after the policy work is scheduled.
