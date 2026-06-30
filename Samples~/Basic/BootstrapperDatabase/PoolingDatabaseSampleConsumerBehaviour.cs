namespace SWeetJelly.Samples.Pooling
{
    using System;
    using System.Collections.Generic;
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using SWeetJelly.Pooling.Contract;
    using SWeetJelly.Pooling.Framework.Unity;
    using UnityEngine;

    public sealed class PoolingDatabaseSampleConsumerBehaviour : MonoBehaviour
    {
        private static readonly PoolId SamplePoolId = "sample.database.cube";

        private readonly List<GameObject> spawned = new List<GameObject>();

        private IGameObjectPoolService poolService;
        private string status = "Waiting for Pooling service...";
        private int spawnIndex;

        private void OnEnable()
        {
            BootstrapperBehaviour.Bootstrapped += OnBootstrapped;
            ResolveIfReady();
        }

        private void OnDisable()
        {
            BootstrapperBehaviour.Bootstrapped -= OnBootstrapped;
        }

        private void OnBootstrapped(object sender, EventArgs args)
        {
            ResolveIfReady();
        }

        private void ResolveIfReady()
        {
            if (poolService != null || !BootstrapperBehaviour.IsBootstrapped || BootstrapperBehaviour.Services == null)
                return;

            if (!BootstrapperBehaviour.Services.TryResolve<IGameObjectPoolService>(out poolService))
            {
                status = "IGameObjectPoolService is not registered.";
                return;
            }

            status = poolService.HasPool(SamplePoolId)
                ? "Pooling service is ready from database."
                : $"Pool not found: {SamplePoolId}";
        }

        private void OnGUI()
        {
            const float x = 16f;
            var y = 16f;

            GUI.Label(new Rect(x, y, 660f, 24f), status);
            y += 28f;

            GUI.Label(new Rect(x, y, 660f, 24f), FormatPoolState());
            y += 36f;

            GUI.enabled = poolService != null && poolService.HasPool(SamplePoolId);

            if (GUI.Button(new Rect(x, y, 190f, 28f), "Spawn From Database"))
                Spawn();

            y += 34f;

            GUI.enabled = spawned.Count > 0;

            if (GUI.Button(new Rect(x, y, 190f, 28f), "Despawn Last"))
                DespawnLast();

            y += 34f;

            GUI.enabled = poolService != null;

            if (GUI.Button(new Rect(x, y, 190f, 28f), "Return All"))
            {
                poolService.ReturnToPoolEverything(resetParent: true);
                spawned.Clear();
                status = "Returned all objects to the database-configured pool.";
            }

            GUI.enabled = true;
        }

        private void Spawn()
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

            EnsureVisible(instance);
            spawned.Add(instance);
            spawnIndex++;
            status = $"Spawned from database pool: {instance.name}";
        }

        private void DespawnLast()
        {
            var lastIndex = spawned.Count - 1;
            var instance = spawned[lastIndex];
            spawned.RemoveAt(lastIndex);

            poolService.Despawn(instance);
            status = $"Despawned: {instance.name}";
        }

        private static void EnsureVisible(GameObject instance)
        {
            if (instance.transform.Find("Runtime Visual") != null)
                return;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Runtime Visual";
            visual.transform.SetParent(instance.transform, false);
            visual.transform.localScale = Vector3.one;

            var renderer = visual.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = new Color(0.95f, 0.7f, 0.25f);
        }

        private string FormatPoolState()
        {
            if (poolService == null || !poolService.TryGetPool(SamplePoolId, out var pool))
                return "Pool: none";

            return $"Pool: {pool.Name}   Initial Size: {pool.InitialSize}   Active Tracked By Sample: {spawned.Count}   Can Grow: {pool.CanIncreaseSize}";
        }
    }
}
