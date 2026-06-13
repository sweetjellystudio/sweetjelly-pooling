# SWeetJelly Pooling

`SWeetJelly Pooling`은 Unity 프로젝트를 위한 재사용 가능한 object pooling을 제공합니다.

이 패키지는 Unity에 의존하지 않는 generic object pool과, projectile, VFX, popup item, board cell, floating text, tile, transition effect처럼 반복적으로 생성/해제되는 GameObject lifecycle을 위한 Unity-facing GameObject pool service를 포함합니다.

## 목적

자주 spawn되는 object를 매번 allocate/destroy하지 않고 재사용해야 할 때 사용합니다.

이 패키지는 모든 lifecycle 결정을 소유하는 단일 global pool manager가 되는 것을 목표로 하지 않습니다. 명시적으로 구성, 등록, 소비할 수 있는 pool과 service를 제공합니다.

## 패키지 구조

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

  InitialiserIntegration/
    PoolInitModule.cs
```

## 어셈블리

### `SWeetJelly.Pooling.Contract`

공개 pool contract를 정의합니다.

- `IPool<T>`
- `IPoolable`
- `PoolId`

### `SWeetJelly.Pooling.Domain`

Unity에 의존하지 않는 pooling logic을 포함합니다.

- `ObjectPool<T>`

### `SWeetJelly.Pooling.Framework`

Unity GameObject pooling 구현을 포함합니다.

- `GameObjectPool`
- `GameObjectPoolServiceBehaviour`
- `IGameObjectPoolService`
- pool database/config asset
- spawn setting

### `SWeetJelly.Pooling.InitialiserIntegration`

Pooling을 Initialiser package와 연결합니다.

- `PoolInitModule`

## Generic Object Pool

순수 C# object에는 `ObjectPool<T>`를 사용합니다.

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

`maxSize`는 pool growth를 제어합니다.

- `maxSize <= 0`: 제한 없이 growth
- `maxSize > 0`: pool이 가득 차고 모든 item이 active이면 `TryRent`가 `false` 반환

## GameObject Pool

Unity GameObject pooling에는 `IGameObjectPoolService`를 사용합니다.

```csharp
using SWeetJelly.Pooling.Contract;
using SWeetJelly.Pooling.Framework;

GameObject instance = poolService.Spawn("projectile");

poolService.Despawn(instance);
```

spawn setting 사용:

```csharp
var settings = new PooledGameObjectSettings()
    .SetPosition(spawnPosition)
    .SetLocalScale(1.25f);

GameObject instance = poolService.Spawn("projectile", settings);
```

typed component spawning도 지원합니다.

```csharp
ProjectileView projectile = poolService.Spawn<ProjectileView>("projectile");
```

## Pool Database

`GameObjectPoolDatabaseSo`는 pool configuration을 저장합니다.

생성 경로:

```text
Create > SWeetJelly > Pooling > GameObject Pool Database
```

각 `GameObjectPoolConfig`는 다음을 정의합니다.

- `poolId`
- pool type: `Single` 또는 `Multi`
- single prefab 또는 weighted multi prefab
- initial size
- pool growth 허용 여부
- optional object container

## Initialiser 설정

`PoolInitModule`은 pooling을 Initialiser system에 설치합니다.

설치 시 다음을 수행합니다.

1. Initialiser holder object에서 `GameObjectPoolServiceBehaviour`를 찾거나 생성
2. `GameObjectPoolDatabaseSo`로 초기화
3. 설정된 pool build
4. `IGameObjectPoolService` 등록

설정 흐름:

1. `GameObjectPoolDatabaseSo` 생성
2. 하나 이상의 pool config 추가
3. `PoolInitModule` 생성
4. module에 database 할당
5. `ProjectInitSettingSo`에 module 추가
6. Initialiser startup 이후 `IGameObjectPoolService` resolve

```csharp
using SWeetJelly.Initialiser.Framework;
using SWeetJelly.Pooling.Framework;

var poolService = InitialiserBehaviour.Services.Resolve<IGameObjectPoolService>();
```

## Object 반환

spawn된 object는 `Despawn`으로 소유 pool에 반환합니다.

```csharp
poolService.Despawn(instance);
```

`GameObjectPoolServiceBehaviour`는 spawn된 object의 ownership을 추적합니다. service가 spawn하지 않은 object를 반환하면 예외가 발생합니다.

모든 object를 반환하려면 다음을 사용합니다.

```csharp
poolService.ReturnToPoolEverything(resetParent: true);
```

## Single And Multi Pools

### Single

single pool은 하나의 prefab instance를 재사용합니다.

bullet, floating text, popup row 같은 일반적인 경우에 사용합니다.

### Multi

multi pool은 여러 prefab을 가지고 weight에 따라 하나를 선택합니다.

같은 logical pool id에서 variation을 spawn해야 할 때 사용합니다.

## 설계 메모

- `ObjectPool<T>`는 순수 C#이며 Unity에 의존하지 않습니다.
- GameObject pooling은 Framework layer에 위치합니다.
- Unity object pooling이 필요한 project gameplay code는 `IGameObjectPoolService`에 의존하는 것이 좋습니다.
- `projectile`, `floating-text`, `enemy-hit-vfx` 같은 stable pool id를 권장합니다.
- Pool database asset은 Unity-facing configuration이며 domain data가 아닙니다.

## 현재 상태

이 패키지는 현재 다음을 제공합니다.

- generic object pool
- pool id value object
- GameObject single pool
- GameObject weighted multi pool
- GameObject pool database asset
- GameObject pool service behaviour
- Initialiser integration module
