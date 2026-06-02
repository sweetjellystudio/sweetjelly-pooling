# README

pooling

여러 모듈에서 반복적으로 필요한 object lifecycle 최적화 기능

UI, VFX, projectile, tile view, popup, board cell, floating text, transition effect 등

pooling 모듈은 전역 pool manager가 아니라 pool 을 생성하고 관리하는 명시적 서비스여야 한다.

```csharp
var pool = new ObjectPool<MyObject>(
    create: () => new MyObject(),
    onRent: item => item.Reset(),
    onReturn: item => item.Cleanup(),
    initialSize: 10,
    maxSize: 100);

MyObject obj = pool.Rent();

pool.Return(obj);
```

- GameObjectPool
  - 내부에서 ObjectPool<GameObject> 사용

- ComponentPool<T> where T : Component
  - 내부에서 GameObjectPool 또는 ObjectPool<GameObject> 사용