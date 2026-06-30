namespace SWeetJelly.Pooling.Tests.EditMode
{
    using System.Reflection;
    using NUnit.Framework;
    using SWeetJelly.Bootstrapper.App;
    using SWeetJelly.Bootstrapper.Framework.Unity;
    using SWeetJelly.Pooling.Framework.Unity;
    using SWeetJelly.Pooling.Framework.Bootstrapper;
    using UnityEngine;

    public sealed class PoolingPackageIntegrationTest
    {
        [Test]
        public void PoolingPackageBootstrapperSo_RegistersGameObjectPoolServiceAndBuildsPools()
        {
            var holder = new GameObject("Pooling Integration Test Holder");
            var prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var database = ScriptableObject.CreateInstance<GameObjectPoolDatabaseSo>();
            var module = ScriptableObject.CreateInstance<PoolingPackageBootstrapperSo>();

            try
            {
                prefab.name = "Pooling Integration Prefab";
                prefab.SetActive(false);

                var config = new GameObjectPoolConfig();
                SetPrivateField(config, "poolId", "sample");
                SetPrivateField(config, "poolType", GameObjectPoolType.Single);
                SetPrivateField(config, "singlePoolPrefab", prefab);
                SetPrivateField(config, "initialSize", 2);
                SetPrivateField(config, "canIncreaseSize", true);

                SetPrivateField(database, "pools", new[] { config });
                SetPrivateField(module, "database", database);

                var registry = new ServiceRegistry();
                var context = new PackageBootstrapContext(holder, null, null, registry);

                module.Bootstrap(context);

                Assert.That(registry.TryResolve<IGameObjectPoolService>(out var service), Is.True);
                Assert.That(service, Is.Not.Null);
                Assert.That(service.HasPool("sample"), Is.True);

                var spawned = service.Spawn("sample");

                Assert.That(spawned, Is.Not.Null);
                Assert.That(spawned.activeSelf, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(module);
                Object.DestroyImmediate(database);
                Object.DestroyImmediate(prefab);
                Object.DestroyImmediate(holder);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target
                .GetType()
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(field, Is.Not.Null, $"Field not found: {fieldName}");
            field.SetValue(target, value);
        }
    }
}
