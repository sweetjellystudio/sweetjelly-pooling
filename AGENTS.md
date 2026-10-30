#   Pooling package instructions

Read [architecture][architecture] before pool, dependency or lifetime changes, [usage][usage] before consumer/sample changes, and the [lifetime decision][decision] before changing reuse, ownership or failure behavior.

*   Apply DDD responsibility ownership and Clean Architecture dependency direction according to actual package needs. Contract/Domain stay Unity-independent. GameObject pooling is a Unity feature with a Unity-facing contract; keep it distinct from the generic pool. An App assembly is not required merely for symmetry.
*   Preserve public signatures, serialized fields and asset GUIDs unless migration is agreed. Generic pools use explicit active/inactive membership. GameObject pools use activeSelf or the selected activeInHierarchy rule. Do not replace either model or enable IPoolable hooks implicitly.
*   Partial pool/database initialization must reclaim only newly built resources and preserve prior pools. Failed typed Spawn returns the instance before throwing the existing exception. Preserve generic callback exception propagation and rollback behavior.
*   Services own their pooled instances, including instances reparented outside the holder. External containers and unrelated children belong to callers. Bootstrap modules destroy only services they create, unregister only their own registration, and preserve borrowed components. A module asset supports one active context.
*   Run SWeetJelly.Pooling.Tests.EditMode and SWeetJelly.Pooling.Tests.PlayMode separately. Restore Random state, temporary objects and scene setup. Test actual deferred destruction and external-container cleanup in PlayMode; observe both samples and the database/profile Inspector separately.
*   Before release, validate the root license, manifest, actual dependencies/asmdefs, Git contents and metadata/GUID. Validate a minimal consumer using only declared dependencies and test tools. Record exact Editor patch, target, counts and fresh reports. Public Git-tag installation is a separate post-publication check.
*   PRs, commits, pushes, tags, Releases and game updates belong to the user. The installed unity-package coordinator and development-host policy own shared workflow; keep this package understandable without ignored host notes.

[architecture]: ARCHITECTURE.md
[usage]: README.md
[decision]: docs/adr/0001-pool-reuse-and-resource-ownership.md
