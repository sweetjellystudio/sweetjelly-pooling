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
