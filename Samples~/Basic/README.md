# Pooling Samples

This folder contains two Pooling package samples.

## Default

`Default` is a code-first sample.

It creates a prefab and `GameObjectPool` at runtime, registers that pool with `GameObjectPoolServiceBehaviour`, and demonstrates spawn/despawn/reuse without requiring any designer-authored assets.

## BootstrapperDatabase

`BootstrapperDatabase` is an asset-driven sample.

It uses:

- `GameObjectPoolDatabaseSo`
- `PoolingPackageBootstrapperSo`
- `PackageBootstrapperProfileSo`
- `BootstrapperBehaviour`

This sample validates the designer-facing flow: configure a pool in a database asset, install it through Bootstrapper, resolve `IGameObjectPoolService`, then spawn and despawn by `PoolId`.

## 2.0.0 validation notes

Both scenes include a directional light and a contrasting camera background so spawned cubes are visible. They do not require URP camera components or AudioListener. The default sample owns its service directly; the database sample uses PackageBootstrapperProfileSo with PoolingPackageBootstrapperSo. Clear/destruction reclaims owned instances while external containers remain caller-owned. IPoolable hooks are not enabled automatically.
