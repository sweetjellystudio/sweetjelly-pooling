# CHANGELOG

## v2.0.0 (local candidate, pending publication)

* [Changed] Require Unity 6.3 LTS and Bootstrapper 2.0.0; remove unused Utils dependency and reference the authoritative root license.
* [Fixed] Roll back partial pool/database initialization and typed Spawn failures; replace destroyed instances without counting duplicate slots.
* [Fixed] Reclaim owned clones on service destruction and bootstrap shutdown, preserving external containers and borrowed components.
* [Added] Regression and runtime lifecycle tests; maintained agent, architecture and ownership decision documents; corrected Bootstrapper setup and Git installation guidance.
* [Compatibility] Keep public APIs, serialized fields/GUIDs, activation-based GameObject availability, generic membership/callback behavior and opt-in IPoolable usage.

## v1.0.0

* [Added] Added `SWeetJelly.Pooling` as an independent Unity package extracted from the previous Core module.
* [Added] Added GameObject pooling support through dedicated pool service and pool configuration assets.
* [Added] Added pool database and pool object settings for configuring reusable pooled prefabs.
* [Added] Added multi-pool prefab support for managing multiple pooled GameObject entries.
* [Added] Added Bootstrapper integration for registering pooling services during project bootstrap.
* [Changed] Moved pooling logic from the unclassified Core module into dedicated Pooling assemblies.
* [Changed] Focused the package around GameObject pooling while leaving pure C# object pooling available for future refactoring if needed.
* [Changed] Separated Pooling into `Contract`, `App`, `Framework/Unity`, and `Framework/Bootstrapper` layers.
