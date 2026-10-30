namespace SWeetJelly.Pooling.Tests.EditMode
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using NUnit.Framework;
    using SWeetJelly.Bootstrapper.App;
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using SWeetJelly.Pooling.Framework.Bootstrapper;
    using SWeetJelly.Pooling.Framework.Unity;
    using SWeetJelly.Pooling.Domain;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public sealed class PoolingLifecycleRegressionTest
    {
        private readonly List<Object> owned = new();
        private GameObject holder;
        private GameObject prefab;
        private Transform container;
        private GameObjectPoolServiceBehaviour service;

        [SetUp]
        public void SetUp()
        {
            holder = Own(new GameObject("Pool regression holder"));
            prefab = Own(new GameObject("Pool regression prefab"));
            prefab.SetActive(false);
            container = Own(new GameObject("External pool container")).transform;
            service = holder.AddComponent<GameObjectPoolServiceBehaviour>();
        }

        [TearDown]
        public void TearDown()
        {
            for (var index = owned.Count - 1; index >= 0; index--)
                if (owned[index] != null) Object.DestroyImmediate(owned[index]);
            owned.Clear();
        }

        [Test]
        public void TypedSpawnFailure_DoesNotConsumeOnlyAvailableInstance()
        {
            service.AddPool("sample", Pool("sample", 1));
            Assert.Throws<InvalidOperationException>(() => service.Spawn<Camera>("sample"));
            Assert.That(service.Spawn("sample"), Is.Not.Null);
        }

        [Test]
        public void DestroyedInstanceReplacement_DoesNotCountTwiceDuringResize()
        {
            var pool = Pool("sample", 1);
            pool.Initialise();
            Object.DestroyImmediate(pool.GetPooledObject());
            var replacement = pool.GetPooledObject();
            pool.CreatePoolObjects(2);
            var second = pool.GetPooledObject();
            Assert.That(second, Is.Not.Null);
            Assert.That(second, Is.Not.SameAs(replacement));
        }

        [Test]
        public void AddPoolFailure_DoesNotPublishBrokenPool()
        {
            Assert.Throws<InvalidOperationException>(() => service.AddPool("broken", Pool("broken", 1, null)));
            Assert.That(service.HasPool("broken"), Is.False);
        }

        [Test]
        public void InitialisationFailure_RollsBackNewPoolsAndKeepsExistingPool()
        {
            service.AddPool("existing", Pool("existing", 0));
            var database = Database(Config("new", 1, prefab), Config("broken", 1, null));
            Assert.Throws<InvalidOperationException>(() => service.Initialise(database, container));
            Assert.That(service.IsInitialised, Is.False);
            Assert.That(service.HasPool("existing"), Is.True);
            Assert.That(service.HasPool("new"), Is.False);
            Assert.That(service.HasPool("broken"), Is.False);
            Assert.That(container.childCount, Is.Zero);
        }

        [Test]
        public void FailedBootstrapRegistration_DoesNotLeaveCreatedComponent()
        {
            Object.DestroyImmediate(service);
            var registry = new ServiceRegistry();
            var otherHolder = Own(new GameObject("Other service owner"));
            var otherService = otherHolder.AddComponent<GameObjectPoolServiceBehaviour>();
            registry.Register<IGameObjectPoolService>(otherService);
            var module = Module(Database(Config("sample", 0, prefab)));
            var context = new PackageBootstrapContext(holder, null, null, registry);
            Assert.Throws<ArgumentException>(() => module.Bootstrap(context));
            Assert.That(holder.GetComponent<GameObjectPoolServiceBehaviour>(), Is.Null);
            Assert.That(registry.Resolve<IGameObjectPoolService>(), Is.SameAs(otherService));
        }

        [Test]
        public void BootstrapShutdown_UnregistersOwnedService()
        {
            Object.DestroyImmediate(service);
            var registry = new ServiceRegistry();
            var module = Module(Database(Config("sample", 0, prefab)));
            var context = new PackageBootstrapContext(holder, null, null, registry);
            module.Bootstrap(context);
            module.Shutdown(context);
            Assert.That(registry.TryResolve<IGameObjectPoolService>(out _), Is.False);
            Assert.That(holder.GetComponent<GameObjectPoolServiceBehaviour>(), Is.Null);
        }

        [Test]
        public void ShutdownBeforeBootstrap_PreservesUnrelatedRegistration()
        {
            var registry = new ServiceRegistry();
            registry.Register<IGameObjectPoolService>(service);
            Module(Database()).Shutdown(new PackageBootstrapContext(holder, null, null, registry));
            Assert.That(registry.Resolve<IGameObjectPoolService>(), Is.SameAs(service));
        }

        [Test]
        public void BootstrapShutdown_PreservesBorrowedInitialisedService()
        {
            service.Initialise(Database(Config("existing", 0, prefab)), container);
            var module = Module(Database(Config("unused", 0, prefab)));
            var registry = new ServiceRegistry();
            var context = new PackageBootstrapContext(holder, null, null, registry);
            module.Bootstrap(context);
            module.Shutdown(context);
            Assert.That(service != null && service.IsInitialised, Is.True);
            Assert.That(service.HasPool("existing"), Is.True);
            Assert.That(registry.TryResolve<IGameObjectPoolService>(out _), Is.False);
        }

        [Test]
        public void FailedRepeatedBootstrap_DoesNotLoseOriginalOwnership()
        {
            Object.DestroyImmediate(service);
            var module = Module(Database(Config("sample", 0, prefab)));
            var registry = new ServiceRegistry();
            var context = new PackageBootstrapContext(holder, null, null, registry);
            module.Bootstrap(context);
            var original = registry.Resolve<IGameObjectPoolService>();
            Assert.Throws<ArgumentException>(() => module.Bootstrap(context));
            Assert.That(registry.Resolve<IGameObjectPoolService>(), Is.SameAs(original));
            module.Shutdown(context);
            Assert.That(registry.TryResolve<IGameObjectPoolService>(out _), Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Availability_KeepsSelectedActivationRule(bool useHierarchy)
        {
            container.gameObject.SetActive(false);
            var pool = Pool("sample", 1);
            pool.Initialise();
            var settings = new PooledGameObjectSettings(true, useHierarchy);
            var first = pool.GetPooledObject(settings);
            var second = pool.GetPooledObject(settings);
            Assert.That(second, useHierarchy ? Is.SameAs(first) : Is.Null);
        }

        [Test]
        public void FailedInitialisation_RemovesOnlyItsDefaultContainer()
        {
            Assert.Throws<InvalidOperationException>(() => service.Initialise(
                Database(Config("new", 1, prefab), Config("broken", 1, null))));
            Assert.That(holder.transform.childCount, Is.Zero);
            Assert.That(service.IsInitialised, Is.False);
        }

        [Test]
        public void Shutdown_DoesNotUnregisterReplacementService()
        {
            Object.DestroyImmediate(service);
            var module = Module(Database(Config("sample", 0, prefab)));
            var registry = new ServiceRegistry();
            var context = new PackageBootstrapContext(holder, null, null, registry);
            module.Bootstrap(context);
            registry.Unregister<IGameObjectPoolService>();
            var other = Own(new GameObject("Replacement owner")).AddComponent<GameObjectPoolServiceBehaviour>();
            registry.Register<IGameObjectPoolService>(other);
            module.Shutdown(context);
            Assert.That(registry.Resolve<IGameObjectPoolService>(), Is.SameAs(other));
        }

        [Test]
        public void MultiPool_UsesPositiveWeightsAndAllowsExplicitIndex()
        {
            var other = Own(new GameObject("Only positive-weight source"));
            other.SetActive(false);
            var pool = new GameObjectPool(new GameObjectPoolBuildArgs("multi", GameObjectPoolType.Multi, null,
                new List<GameObjectPoolPrefabEntry> { new(prefab, 0), new(other, 100) }, 1, false, container));
            var randomState = UnityEngine.Random.state;
            try
            {
                for (var i = 0; i < 20; i++)
                {
                    Assert.That(pool.GetPooledObject().name, Does.StartWith(other.name));
                    pool.ReturnToPoolEverything();
                }
                Assert.That(pool.GetMultiPooledObjectByIndex(0, new PooledGameObjectSettings()).name,
                    Does.StartWith(prefab.name));
            }
            finally { UnityEngine.Random.state = randomState; }
        }

        [Test]
        public void ForeignDespawn_KeepsExistingErrorContract()
        {
            Assert.Throws<InvalidOperationException>(() => service.Despawn(prefab));
        }

        [Test]
        public void GenericRentCallbackFailure_ReturnsItemToInactiveSet()
        {
            var fail = true;
            var expected = new InvalidOperationException("Rent callback");
            var item = new object();
            var pool = new ObjectPool<object>(() => item, onRent: _ => { if (fail) throw expected; }, initialSize: 1);
            Assert.That(Assert.Throws<InvalidOperationException>(() => pool.Rent()), Is.SameAs(expected));
            Assert.That(pool.ActiveCount, Is.Zero);
            Assert.That(pool.InactiveCount, Is.EqualTo(1));
            fail = false;
            Assert.That(pool.Rent(), Is.SameAs(item));
        }

        [Test]
        public void GenericReturnCallbackFailure_KeepsItemActive()
        {
            var fail = true;
            var pool = new ObjectPool<object>(() => new object(), onReturn: _ =>
                { if (fail) throw new InvalidOperationException("Return callback"); });
            var item = pool.Rent();
            Assert.Throws<InvalidOperationException>(() => pool.Return(item));
            Assert.That(pool.IsActive(item), Is.True);
            fail = false;
            pool.Return(item);
            Assert.That(pool.ActiveCount, Is.Zero);
            Assert.That(pool.InactiveCount, Is.EqualTo(1));
        }

        [Test]
        public void GenericClear_DestroysActiveAndInactiveItemsOnce()
        {
            var destroyed = new HashSet<object>();
            var pool = new ObjectPool<object>(() => new object(), onDestroy: item =>
                Assert.That(destroyed.Add(item), Is.True), initialSize: 3);
            pool.Rent();
            pool.Clear();
            Assert.That(destroyed.Count, Is.EqualTo(3));
            Assert.That(pool.Count, Is.Zero);
        }

        private GameObjectPool Pool(string name, int count) => Pool(name, count, prefab);
        private GameObjectPool Pool(string name, int count, GameObject source) => new(
            new GameObjectPoolBuildArgs(name, GameObjectPoolType.Single, source,
                new List<GameObjectPoolPrefabEntry>(), count, false, container));

        private GameObjectPoolConfig Config(string id, int count, GameObject source)
        {
            var config = new GameObjectPoolConfig();
            Set(config, "poolId", id);
            Set(config, "singlePoolPrefab", source);
            Set(config, "initialSize", count);
            return config;
        }

        private GameObjectPoolDatabaseSo Database(params GameObjectPoolConfig[] configs)
        {
            var database = Own(ScriptableObject.CreateInstance<GameObjectPoolDatabaseSo>());
            Set(database, "pools", configs);
            return database;
        }

        private PoolingPackageBootstrapperSo Module(GameObjectPoolDatabaseSo database)
        {
            var module = Own(ScriptableObject.CreateInstance<PoolingPackageBootstrapperSo>());
            Set(module, "database", database);
            return module;
        }

        private T Own<T>(T value) where T : Object { owned.Add(value); return value; }
        private static void Set(object target, string name, object value) => target.GetType()
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
