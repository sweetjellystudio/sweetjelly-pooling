namespace SWeetJelly.Pooling.Tests.EditMode
{
    using System.Collections.Generic;
    using NUnit.Framework;
    using SWeetJelly.Pooling.Framework.Unity;
    using UnityEngine;

    public sealed class GameObjectPoolServiceTest
    {
        private GameObject holder;
        private GameObject prefab;

        [TearDown]
        public void TearDown()
        {
            if (holder != null)
                Object.DestroyImmediate(holder);

            if (prefab != null)
                Object.DestroyImmediate(prefab);
        }

        [Test]
        public void SpawnAndDespawn_ReusesInstance()
        {
            var service = CreateServiceWithPool(initialSize: 1, canIncreaseSize: false);

            var first = service.Spawn("sample");

            Assert.That(first, Is.Not.Null);
            Assert.That(first.activeSelf, Is.True);

            service.Despawn(first);

            Assert.That(first.activeSelf, Is.False);

            var second = service.Spawn("sample");

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void ReturnToPoolEverything_DeactivatesSpawnedObjects()
        {
            var service = CreateServiceWithPool(initialSize: 2, canIncreaseSize: false);

            var first = service.Spawn("sample");
            var second = service.Spawn("sample");

            service.ReturnToPoolEverything();

            Assert.That(first.activeSelf, Is.False);
            Assert.That(second.activeSelf, Is.False);
        }

        private GameObjectPoolServiceBehaviour CreateServiceWithPool(
            int initialSize,
            bool canIncreaseSize)
        {
            holder = new GameObject("Pooling Service Test Holder");
            prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            prefab.name = "Pooling Test Prefab";
            prefab.SetActive(false);

            var container = new GameObject("Pool Container").transform;
            container.SetParent(holder.transform, false);

            var service = holder.AddComponent<GameObjectPoolServiceBehaviour>();
            var args = new GameObjectPoolBuildArgs(
                "sample",
                GameObjectPoolType.Single,
                prefab,
                new List<GameObjectPoolPrefabEntry>(),
                initialSize,
                canIncreaseSize,
                container);

            service.AddPool("sample", new GameObjectPool(args));

            return service;
        }
    }
}
