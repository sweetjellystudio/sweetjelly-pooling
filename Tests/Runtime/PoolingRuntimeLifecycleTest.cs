namespace SWeetJelly.Pooling.Tests.PlayMode
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using NUnit.Framework;
    using SWeetJelly.Bootstrapper.App;
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using SWeetJelly.Pooling.Framework.Bootstrapper;
    using SWeetJelly.Pooling.Framework.Unity;
    using UnityEngine;
    using UnityEngine.TestTools;
    using Object = UnityEngine.Object;

    public sealed class PoolingRuntimeLifecycleTest
    {
        private readonly List<Object> owned = new();
        private GameObject holder;
        private GameObject prefab;
        private Transform external;

        [SetUp]
        public void SetUp()
        {
            holder = Own(new GameObject("Runtime pool holder"));
            prefab = Own(new GameObject("Runtime pool source"));
            prefab.SetActive(false);
            external = Own(new GameObject("Caller-owned external container")).transform;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            for (var i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) Object.Destroy(owned[i]);
            owned.Clear();
            yield return null;
        }

        [UnityTest]
        public IEnumerator DestroyingService_ReclaimsExternalInstancesButPreservesContainer()
        {
            var service = holder.AddComponent<GameObjectPoolServiceBehaviour>();
            service.AddPool("sample", Pool());
            var instance = service.Spawn("sample");
            Object.Destroy(service);
            yield return null;
            Assert.That(instance == null, Is.True);
            Assert.That(external != null, Is.True);
            Assert.That(holder != null, Is.True);
        }

        [UnityTest]
        public IEnumerator DestroyingHolder_PreservesUnrelatedExternalChildren()
        {
            var sentinel = new GameObject("Caller-owned child");
            sentinel.transform.SetParent(external, false);
            var service = holder.AddComponent<GameObjectPoolServiceBehaviour>();
            service.AddPool("sample", Pool());
            var instance = service.Spawn("sample");
            Object.Destroy(holder);
            yield return null;
            Assert.That(instance == null, Is.True);
            Assert.That(sentinel != null && external != null, Is.True);
        }

        [UnityTest]
        public IEnumerator ClearAndRebuild_DoesNotReusePendingDestruction()
        {
            var service = holder.AddComponent<GameObjectPoolServiceBehaviour>();
            service.AddPool("sample", Pool());
            var old = service.Spawn("sample");
            service.Clear();
            Assert.That(old.activeSelf, Is.False);
            service.AddPool("sample", Pool());
            var current = service.Spawn("sample");
            Assert.That(current, Is.Not.SameAs(old));
            yield return null;
            Assert.That(old == null, Is.True);
            Assert.That(current != null && current.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator BootstrapShutdown_ReclaimsSpawnReparentedOutsideHolder()
        {
            var database = Own(ScriptableObject.CreateInstance<GameObjectPoolDatabaseSo>());
            var config = new GameObjectPoolConfig();
            Set(config, "poolId", "sample");
            Set(config, "singlePoolPrefab", prefab);
            Set(config, "initialSize", 1);
            Set(database, "pools", new[] { config });
            var module = Own(ScriptableObject.CreateInstance<PoolingPackageBootstrapperSo>());
            Set(module, "database", database);
            var registry = new ServiceRegistry();
            var context = new PackageBootstrapContext(holder, null, null, registry);
            module.Bootstrap(context);
            var instance = registry.Resolve<IGameObjectPoolService>().Spawn("sample",
                new PooledGameObjectSettings(true, false).SetParent(external));
            module.Shutdown(context);
            Assert.That(registry.TryResolve<IGameObjectPoolService>(out _), Is.False);
            Assert.That(instance.activeSelf, Is.False);
            yield return null;
            Assert.That(instance == null, Is.True);
            Assert.That(holder.GetComponent<GameObjectPoolServiceBehaviour>(), Is.Null);
            Assert.That(external != null, Is.True);
        }

        private GameObjectPool Pool() => new(new GameObjectPoolBuildArgs("sample", GameObjectPoolType.Single,
            prefab, new List<GameObjectPoolPrefabEntry>(), 1, false, external));
        private T Own<T>(T value) where T : Object { owned.Add(value); return value; }
        private static void Set(object target, string name, object value) => target.GetType()
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
