# Pooling Dev Sample

This sample shows the consumer-side flow for `com.sweetjelly.pooling`.

It creates a small cube prefab at runtime, builds a `GameObjectPool`, registers it with `GameObjectPoolServiceBehaviour`, then spawns and despawns instances through `IGameObjectPoolService`.

## Setup

Open `Sample Scene` and enter Play Mode.

Use the on-screen buttons to spawn cubes, despawn the last spawned cube, return all spawned cubes to the pool, and clear/rebuild the pool.

This sample uses runtime setup to keep the sample self-contained. Production projects can also configure pools through `GameObjectPoolDatabaseSo` and install them with `PoolingPackageBootstrapperSo`.
