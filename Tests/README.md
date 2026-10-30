# Pooling Tests

This folder validates `com.sweetjelly.pooling` in the current Unity dev project.

## EditMode

EditMode tests cover pure pool logic, Unity GameObject pool service behavior, and Bootstrapper integration.

- `ObjectPoolTest`
  - verifies prewarm count, rent/return counts, item reuse, max-size behavior, and invalid return handling.

- `GameObjectPoolServiceTest`
  - verifies runtime GameObject pool registration, spawn, despawn, and return-all behavior.

- `PoolingPackageIntegrationTest`
  - verifies `PoolingPackageBootstrapperSo` creates or reuses `GameObjectPoolServiceBehaviour`, builds pools from a database, and registers `IGameObjectPoolService`.

The tests avoid relying on persistent scene assets. Prefabs, database assets, and pool configs are created in test code.

## 2.0.0 regression and runtime checks

PoolingLifecycleRegressionTest covers typed Spawn recovery, destroyed-slot replacement, transactional pool/database initialization, preserved previous pools/default-container cleanup, bootstrap registration ownership/retry/shutdown, weighted and explicit selection, activation rules, foreign returns and generic callback rollback/clear. PoolingRuntimeLifecycleTest covers actual deferred destruction, external-parent instance cleanup, caller-owned children, clear/rebuild and bootstrap shutdown after reparenting.

Run SWeetJelly.Pooling.Tests.EditMode and SWeetJelly.Pooling.Tests.PlayMode separately. Record the actual Unity 6.3 patch, target, executed/passed/failed/skipped counts and fresh reports. Observe both imported samples and database/profile Inspector independently. A minimal consumer uses pooling, bootstrapper's public Git tag and test tools; build both sample scenes into a Windows Player. Local candidate success does not verify the future public pooling tag or other platforms/backends.
