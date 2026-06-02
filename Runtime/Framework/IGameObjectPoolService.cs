namespace SWeetJelly.Pooling.Framework
{
    using UnityEngine;
    using SWeetJelly.Pooling.Contract;

    public interface IGameObjectPoolService
    {
        bool HasPool(PoolId poolId);

        GameObjectPool GetPool(PoolId poolId);

        bool TryGetPool(PoolId poolId, out GameObjectPool pool);

        GameObject Spawn(PoolId poolId);

        GameObject Spawn(PoolId poolId, PooledGameObjectSettings settings);

        T Spawn<T>(PoolId poolId)
            where T : Component;

        T Spawn<T>(PoolId poolId, PooledGameObjectSettings settings)
            where T : Component;

        void Despawn(GameObject instance);

        void ReturnToPoolEverything(bool resetParent = false);

        void Clear();
    }
}
