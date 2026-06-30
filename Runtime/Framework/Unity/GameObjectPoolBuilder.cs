using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SWeetJelly.Pooling.Framework.Unity
{
    public class GameObjectPoolBuilder
    {
        private string name = string.Empty;
        private GameObjectPoolType type = GameObjectPoolType.Single;
        private GameObject singlePrefab;
        private List<GameObjectPoolPrefabEntry> multiPrefabs = new();
        private int initialSize = 10;
        private bool canIncreaseSize = true;
        private Transform objectsContainer;

        public GameObjectPoolBuilder SetName(string value)
        {
            name = value ?? string.Empty;
            return this;
        }

        public GameObjectPoolBuilder SetType(GameObjectPoolType value)
        {
            type = value;
            return this;
        }

        public GameObjectPoolBuilder SetSinglePrefab(GameObject prefab)
        {
            singlePrefab = prefab;

            return this;
        }

        public GameObjectPoolBuilder SetMultiPrefabs(IEnumerable<GameObjectPoolPrefabEntry> entries)
        {
            multiPrefabs = entries != null ? new List<GameObjectPoolPrefabEntry>(entries) : new List<GameObjectPoolPrefabEntry>();

            return this;
        }

        public GameObjectPoolBuilder AddMultiPrefab(GameObject prefab, int weight, bool isWeightLocked = false)
        {
            multiPrefabs.Add(new GameObjectPoolPrefabEntry(prefab, weight, isWeightLocked));

            return this;
        }

        public GameObjectPoolBuilder SetInitialSize(int value)
        {
            initialSize = value;

            return this;
        }

        public GameObjectPoolBuilder SetCanIncreaseSize(bool value)
        {
            canIncreaseSize = value;

            return this;
        }

        public GameObjectPoolBuilder SetObjectsContainer(Transform value)
        {
            objectsContainer = value;

            return this;
        }

        public GameObjectPool Build()
        {
            var args = new GameObjectPoolBuildArgs(
                name,
                type,
                singlePrefab,
                multiPrefabs,
                initialSize,
                canIncreaseSize,
                objectsContainer);

            return new GameObjectPool(args);
        }

        public GameObjectPoolBuilder Reset()
        {
            name = string.Empty;
            type = GameObjectPoolType.Single;
            singlePrefab = null;
            multiPrefabs = new List<GameObjectPoolPrefabEntry>();
            initialSize = 10;
            canIncreaseSize = true;
            objectsContainer = null;

            return this;
        }

        public GameObjectPool BuildFromConfig(GameObjectPoolConfig config, Transform defaultContainer = null)
        {
            var args = new GameObjectPoolBuildArgs(
                config.PoolName,
                config.PoolType,
                config.SinglePoolPrefab,
                config.MultiPoolPrefabs.ToList(),
                config.InitialSize,
                config.CanIncreaseSize,
                config.ObjectsContainer != null ? config.ObjectsContainer : defaultContainer);

            return new GameObjectPool(args);
        }

        public GameObjectPool BuildSinglePool(
            string name,
            GameObject prefab,
            int initialSize = 10,
            bool canIncreaseSize = true,
            Transform objectsContainer = null)
        {
            return Reset()
                .SetName(name)
                .SetType(GameObjectPoolType.Single)
                .SetSinglePrefab(prefab)
                .SetInitialSize(initialSize)
                .SetCanIncreaseSize(canIncreaseSize)
                .SetObjectsContainer(objectsContainer)
                .Build();
        }

        public GameObjectPool BuildMultiPool(
            string name,
            IEnumerable<GameObjectPoolPrefabEntry> multiPrefabs,
            int initialSize = 10,
            bool canIncreaseSize = true,
            Transform objectsContainer = null)
        {
            return Reset()
                .SetName(name)
                .SetType(GameObjectPoolType.Multi)
                .SetMultiPrefabs(multiPrefabs)
                .SetInitialSize(initialSize)
                .SetCanIncreaseSize(canIncreaseSize)
                .SetObjectsContainer(objectsContainer)
                .Build();
        }
    }
}