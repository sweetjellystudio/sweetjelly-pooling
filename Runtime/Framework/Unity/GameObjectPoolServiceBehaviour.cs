namespace SWeetJelly.Pooling.Framework.Unity
{
    using System;
    using System.Collections.Generic;
    using SWeetJelly.Pooling.Contract;
    using UnityEngine;

    public sealed class GameObjectPoolServiceBehaviour : MonoBehaviour, IGameObjectPoolService
    {
        [SerializeField] private GameObjectPoolDatabaseSo database;
        [SerializeField] private Transform defaultContainer;

        private readonly Dictionary<PoolId, GameObjectPool> pools = new();
        private readonly Dictionary<GameObject, GameObjectPool> owners = new();

        private readonly GameObjectPoolBuilder builder = new();
        private Transform ownedContainer;

        public bool IsInitialised { get; private set; }

        public void Initialise(GameObjectPoolDatabaseSo poolDatabase, Transform container = null)
        {
            if (IsInitialised)
                return;

            var previousDatabase = database;
            var previousContainer = defaultContainer;
            var previousPoolIds = new HashSet<PoolId>(pools.Keys);
            var createdContainer = false;
            database = poolDatabase;
            defaultContainer = container != null ? container : defaultContainer;

            try
            {
                if (defaultContainer == null)
                {
                    defaultContainer = CreateDefaultContainer();
                    ownedContainer = defaultContainer;
                    createdContainer = true;
                }
                BuildPoolsFromDatabase();
                IsInitialised = true;
            }
            catch
            {
                foreach (var id in new List<PoolId>(pools.Keys))
                {
                    if (previousPoolIds.Contains(id)) continue;
                    pools[id].Clear();
                    pools.Remove(id);
                }
                if (createdContainer)
                {
                    DestroyOwnedObject(ownedContainer.gameObject);
                    ownedContainer = null;
                }
                database = previousDatabase;
                defaultContainer = previousContainer;
                throw;
            }
        }

        public bool HasPool(PoolId poolId)
        {
            return pools.ContainsKey(poolId);
        }

        public GameObjectPool GetPool(PoolId poolId)
        {
            if (TryGetPool(poolId, out GameObjectPool pool))
                return pool;

            throw new InvalidOperationException($"Pool not found: {poolId}");
        }

        public bool TryGetPool(PoolId poolId, out GameObjectPool pool)
        {
            return pools.TryGetValue(poolId, out pool);
        }

        public void AddPool(PoolId poolId, GameObjectPool pool)
        {
            if (poolId.IsEmpty)
                throw new ArgumentException("Pool id cannot be empty.", nameof(poolId));

            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            if (pools.ContainsKey(poolId))
                throw new InvalidOperationException($"Pool already exists: {poolId}");

            pool.Initialise();
            pools.Add(poolId, pool);
        }

        public GameObject Spawn(PoolId poolId)
        {
            return Spawn(poolId, new PooledGameObjectSettings(true, false));
        }

        public GameObject Spawn(PoolId poolId, PooledGameObjectSettings settings)
        {
            GameObjectPool pool = GetPool(poolId);

            GameObject instance = pool.GetPooledObject(settings);

            if (instance == null)
                return null;

            owners[instance] = pool;
            return instance;
        }

        public T Spawn<T>(PoolId poolId)
            where T : Component
        {
            return Spawn<T>(poolId, new PooledGameObjectSettings(true, false));
        }

        public T Spawn<T>(PoolId poolId, PooledGameObjectSettings settings)
            where T : Component
        {
            GameObject instance = Spawn(poolId, settings);

            if (instance == null)
                return null;

            if (!instance.TryGetComponent(out T component))
            {
                Despawn(instance);
                throw new InvalidOperationException(
                    $"Spawned object from pool '{poolId}' does not contain component '{typeof(T).Name}'.");
            }

            return component;
        }

        public void Despawn(GameObject instance)
        {
            if (instance == null)
                return;

            if (!owners.TryGetValue(instance, out GameObjectPool pool))
            {
                throw new InvalidOperationException(
                    $"Object '{instance.name}' is not owned by this pool service.");
            }

            owners.Remove(instance);

            // 현재 GameObjectPool이 activeSelf 기반으로 available 상태를 판단하므로
            // 여기서는 SetActive(false)가 반환 처리다.
            instance.SetActive(false);

            if (pool.ObjectsContainer != null)
                instance.transform.SetParent(pool.ObjectsContainer, false);
        }

        public void ReturnToPoolEverything(bool resetParent = false)
        {
            foreach (GameObjectPool pool in pools.Values)
            {
                pool.ReturnToPoolEverything(resetParent);
            }

            owners.Clear();
        }

        public void Clear()
        {
            foreach (GameObjectPool pool in pools.Values)
            {
                pool.Clear();
            }

            pools.Clear();
            owners.Clear();
            IsInitialised = false;
            if (ownedContainer != null)
            {
                if (defaultContainer == ownedContainer) defaultContainer = null;
                DestroyOwnedObject(ownedContainer.gameObject);
                ownedContainer = null;
            }
        }

        private void OnDestroy()
        {
            Clear();
        }

        private static void DestroyOwnedObject(GameObject value)
        {
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }

        private void BuildPoolsFromDatabase()
        {
            if (database == null)
            {
                Debug.LogError("[PoolService] PoolDatabase is not assigned.");
                return;
            }

            foreach (GameObjectPoolConfig config in database.Pools)
            {
                if (config == null)
                    continue;

                PoolId poolId = config.PoolId;

                GameObjectPool pool = builder.BuildFromConfig(
                    config,
                    defaultContainer);

                AddPool(poolId, pool);
            }
        }

        private Transform CreateDefaultContainer()
        {
            var root = new GameObject("[Pools]");
            root.transform.SetParent(transform, false);
            return root.transform;
        }
    }
}