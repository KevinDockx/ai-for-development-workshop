# Document storage for first ClaimHub release

30 January 2025

**Attendees:** Sofie Vermeulen, Tom De Smet, Nadia Peeters, Bram Jacobs

The blob-storage proposal will not be ready for the first release. Procurement, retention configuration and ownership of the cloud subscription are all still open. Claims Operations still needs photographs and invoices in the release.

We will store documents on local disk beneath a configured root. “Local” means local to the deployed service or its mounted volume, not a path chosen by the browser. The database stores the document record and generated storage name.

Sofie will arrange for the configured root to be backed up with the database. Bram will keep the storage interface so blob storage can replace the implementation later. Nadia asked that users continue to see the original file name, but it must not determine where the file is written.

Nobody agreed to expose a shared-folder path to staff. All downloads continue through ClaimHub.

## Actions

- Sofie: provide development and production root locations through deployment configuration.
- Bram: implement local-disk storage behind the existing storage abstraction.
- Anouk: cover duplicate names and missing files.
- Nadia: confirm acceptable document types and maximum sizes.
- Tom: record blob storage as deferred, not rejected.
