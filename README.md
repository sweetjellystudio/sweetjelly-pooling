# SWeetJelly Pooling

`SWeetJelly Pooling` provides reusable object pooling for Unity projects.

The package contains a Unity-independent generic object pool and Unity-facing GameObject pool services for repeated object lifecycle scenarios such as projectiles, VFX, popup items, board cells, floating text, tiles, and transition effects.

## Purpose

Use this package when a project needs to reuse frequently spawned objects instead of repeatedly allocating and destroying them.

The package is not intended to be a single global pool manager that owns every lifecycle decision. It provides explicit pools and services that can be configured, registered, and consumed by project code.

## Package Structure

```text
Runtime/
  Contract/
    IPool.cs
    IPoolable.cs
    PoolId.cs

  Domain/
    ObjectPool.cs

  Framework/
    GameObjectPool.cs
    GameObjectPoolBuildArgs.cs
    GameObjectPoolBuilder.cs
    GameObjectPoolConfig.cs
    GameObjectPoolDatabase.cs
    GameObjectPoolPrefabEntry.cs
    GameObjectPoolServiceBehaviour.cs
    GameObjectPoolWeightUtility.cs
    IGameObjectPoolService.cs
    PooledGameObjectSettings.cs

  Framework/Bootstrapper/
    PoolingPackageBootstrapperSo.cs
```

## Assemblies

### `SWeetJelly.Pooling.Contract`

Defines public pool contracts.

- `IPool<T>`
- `IPoolable`
- `PoolId`

### `SWeetJelly.Pooling.Domain`

Contains Unity-independent pooling logic.

- `ObjectPool<T>`

### `SWeetJelly.Pooling.Framework`

Contains Unity GameObject pooling implementation.

- `GameObjectPool`
- `GameObjectPoolServiceBehaviour`
- `IGameObjectPoolService`
- pool database/config assets
- spawn settings

### `SWeetJelly.Pooling.Framework.Bootstrapper`

Connects pooling to the Bootstrapper package.

- `PoolingPackageBootstrapperSo`

## Generic Object Pool

Use `ObjectPool<T>` for pure C# objects.

```csharp
var pool = new ObjectPool<MyObject>(
    create: () => new MyObject(),
    onRent: item => item.Reset(),
    onReturn: item => item.Cleanup(),
    onDestroy: item => item.Dispose(),
    initialSize: 10,
    maxSize: 100);

MyObject item = pool.Rent();

pool.Return(item);
```

`maxSize` controls growth.

- `maxSize <= 0`: unlimited growth
- `maxSize > 0`: `TryRent` returns `false` when the pool is full and all items are active

## GameObject Pool

Use `IGameObjectPoolService` for Unity GameObject pooling.

```csharp
using SWeetJelly.Pooling.Contract;
using SWeetJelly.Pooling.Framework.Unity;

GameObject instance = poolService.Spawn("projectile");

poolService.Despawn(instance);
```

With spawn settings:

```csharp
var settings = new PooledGameObjectSettings()
    .SetPosition(spawnPosition)
    .SetLocalScale(1.25f);

GameObject instance = poolService.Spawn("projectile", settings);
```

Typed component spawning is also supported.

```csharp
ProjectileView projectile = poolService.Spawn<ProjectileView>("projectile");
```

## Pool Database

`GameObjectPoolDatabaseSo` stores pool configurations.

Create it from:

```text
Create > SWeetJelly > Pooling > GameObject Pool Database
```

Each `GameObjectPoolConfig` defines:

- `poolId`
- pool type: `Single` or `Multi`
- single prefab or weighted multi prefabs
- initial size
- whether the pool can grow
- optional object container

## Bootstrapper Setup

`PoolingPackageBootstrapperSo` installs pooling into the Bootstrapper system.

On install it:

1. creates or finds `GameObjectPoolServiceBehaviour` on the Initialiser holder object
2. initialises it with `GameObjectPoolDatabaseSo`
3. builds configured pools
4. registers `IGameObjectPoolService`

Setup flow:

1. Create a `GameObjectPoolDatabaseSo`.
2. Add one or more pool configs.
3. Create `PoolingPackageBootstrapperSo`.
4. Assign the database to the module.
5. Add the module to `ProjectInitSettingSo`.
6. Resolve `IGameObjectPoolService` after Bootstrapper startup.

```csharp
using SWeetJelly.Bootstrapper.Framework.Unity;
using SWeetJelly.Pooling.Framework.Unity;

var poolService = BootstrapperBehaviour.Services.Resolve<IGameObjectPoolService>();
```

## Returning Objects

Use `Despawn` to return a spawned object to its owning pool.

```csharp
poolService.Despawn(instance);
```

`GameObjectPoolServiceBehaviour` tracks spawned object ownership. Returning an object that was not spawned by the service throws an exception.

To return everything:

```csharp
poolService.ReturnToPoolEverything(resetParent: true);
```

## Single And Multi Pools

### Single

A single pool reuses instances of one prefab.

Use this for common cases such as bullets, floating text, or popup rows.

### Multi

A multi pool contains several prefabs and selects one by weight.

Use this when a pool should spawn variations from the same logical pool id.

## Design Notes

- `ObjectPool<T>` is pure C# and does not depend on Unity.
- GameObject pooling lives in the Framework layer.
- Project gameplay code should depend on `IGameObjectPoolService` when it needs Unity object pooling.
- Stable pool ids such as `projectile`, `floating-text`, or `enemy-hit-vfx` are recommended.
- Pool database assets are Unity-facing configuration, not domain data.

## Current Status

This package currently provides:

- generic object pool
- pool id value object
- GameObject single pool
- GameObject weighted multi pool
- GameObject pool database asset
- GameObject pool service behaviour
- Bootstrapper integration module
