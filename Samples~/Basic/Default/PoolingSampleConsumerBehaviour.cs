namespace SWeetJelly.Samples.Pooling
{
    using System.Collections.Generic;
    using SWeetJelly.Pooling.Contract;
    using SWeetJelly.Pooling.Framework.Unity;
    using UnityEngine;

    public sealed class PoolingSampleConsumerBehaviour : MonoBehaviour
    {
        private static readonly PoolId SamplePoolId = "sample.cube";

        private readonly List<GameObject> spawned = new List<GameObject>();

        private GameObjectPoolServiceBehaviour poolService;
        private GameObject prefab;
        private Transform poolContainer;
        private int spawnIndex;
        private string status = "Pool is not bootstrapped.";

        private void Awake()
        {
            BuildSamplePool();
        }

        private void OnDestroy()
        {
            poolService?.Clear();

            if (prefab != null)
                Destroy(prefab);
        }

        private void OnGUI()
        {
            const float x = 16f;
            var y = 16f;

            GUI.Label(new Rect(x, y, 620f, 24f), status);
            y += 28f;

            GUI.Label(new Rect(x, y, 620f, 24f), FormatPoolState());
            y += 36f;

            GUI.enabled = poolService != null && poolService.HasPool(SamplePoolId);

            if (GUI.Button(new Rect(x, y, 190f, 28f), "Spawn Cube"))
                SpawnCube();

            y += 34f;

            GUI.enabled = spawned.Count > 0;

            if (GUI.Button(new Rect(x, y, 190f, 28f), "Despawn Last"))
                DespawnLast();

            y += 34f;

            GUI.enabled = poolService != null;

            if (GUI.Button(new Rect(x, y, 190f, 28f), "Return All"))
                ReturnAll();

            y += 34f;

            if (GUI.Button(new Rect(x, y, 190f, 28f), "Clear And Rebuild"))
            {
                poolService.Clear();
                spawned.Clear();
                BuildSamplePool();
            }

            GUI.enabled = true;
        }

        private void BuildSamplePool()
        {
            poolService = GetComponent<GameObjectPoolServiceBehaviour>();

            if (poolService == null)
                poolService = gameObject.AddComponent<GameObjectPoolServiceBehaviour>();

            if (poolContainer == null)
            {
                var containerObject = new GameObject("Sample Pool Container");
                containerObject.transform.SetParent(transform, false);
                poolContainer = containerObject.transform;
            }

            prefab = prefab != null ? prefab : CreatePrefab();

            var args = new GameObjectPoolBuildArgs(
                SamplePoolId.Value,
                GameObjectPoolType.Single,
                prefab,
                new List<GameObjectPoolPrefabEntry>(),
                initialSize: 3,
                canIncreaseSize: true,
                objectsContainer: poolContainer);

            var pool = new GameObjectPool(args);

            if (!poolService.HasPool(SamplePoolId))
                poolService.AddPool(SamplePoolId, pool);

            status = "Sample cube pool is ready.";
        }

        private GameObject CreatePrefab()
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Pooling Sample Cube Prefab";
            cube.SetActive(false);
            cube.transform.SetParent(transform, false);

            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = new Color(0.25f, 0.75f, 1f);

            return cube;
        }

        private void SpawnCube()
        {
            var x = (spawnIndex % 6) * 1.4f - 3.5f;
            var z = (spawnIndex / 6) * 1.4f;

            var settings = new PooledGameObjectSettings()
                .SetPosition(new Vector3(x, 0f, z))
                .SetLocalScale(0.8f);

            var instance = poolService.Spawn(SamplePoolId, settings);

            if (instance == null)
            {
                status = "Pool returned null.";
                return;
            }

            spawned.Add(instance);
            spawnIndex++;
            status = $"Spawned: {instance.name}";
        }

        private void DespawnLast()
        {
            var lastIndex = spawned.Count - 1;
            var instance = spawned[lastIndex];
            spawned.RemoveAt(lastIndex);

            poolService.Despawn(instance);
            status = $"Despawned: {instance.name}";
        }

        private void ReturnAll()
        {
            poolService.ReturnToPoolEverything(resetParent: true);
            spawned.Clear();
            status = "Returned all objects to the pool.";
        }

        private string FormatPoolState()
        {
            if (poolService == null || !poolService.TryGetPool(SamplePoolId, out var pool))
                return "Pool: none";

            return $"Pool: {pool.Name}   Initial Size: {pool.InitialSize}   Active Tracked By Sample: {spawned.Count}   Can Grow: {pool.CanIncreaseSize}";
        }
    }
}
