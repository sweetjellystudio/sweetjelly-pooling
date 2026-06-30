# Pooling Bootstrapper Database Sample

This sample shows the asset-driven Pooling setup.

- `Sample Cube Prefab.prefab` is assigned to `GameObject Pool Database.asset`.
- `Pool Package Bootstrapper.asset` references the database.
- `Sample Package Bootstrapper Profile.asset` installs the pool module through `BootstrapperBehaviour`.
- `PoolingDatabaseSampleConsumerBehaviour` resolves `IGameObjectPoolService` and spawns/despawns objects by `PoolId`.

Open `Sample Scene` and enter Play Mode.

Use the on-screen buttons to spawn cubes, despawn the last cube, and return all spawned cubes to the pool.
