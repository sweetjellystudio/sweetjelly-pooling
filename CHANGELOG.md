# CHANGELOG

## v1.0.0

* [Added] Added `SWeetJelly.Pooling` as an independent Unity package extracted from the previous Core module.
* [Added] Added GameObject pooling support through dedicated pool service and pool configuration assets.
* [Added] Added pool database and pool object settings for configuring reusable pooled prefabs.
* [Added] Added multi-pool prefab support for managing multiple pooled GameObject entries.
* [Added] Added Bootstrapper integration for registering pooling services during project bootstrap.
* [Changed] Moved pooling logic from the unclassified Core module into dedicated Pooling assemblies.
* [Changed] Focused the package around GameObject pooling while leaving pure C# object pooling available for future refactoring if needed.
* [Changed] Separated Pooling into `Contract`, `App`, `Framework/Unity`, and `Framework/Bootstrapper` layers.
