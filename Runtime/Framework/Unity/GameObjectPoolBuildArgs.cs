using UnityEngine;
using System;
using System.Collections.Generic;

namespace SWeetJelly.Pooling.Framework.Unity
{
    public sealed class GameObjectPoolBuildArgs
    {
        public string Name { get; }
        public GameObjectPoolType Type { get; }
        public GameObject SinglePrefab { get; }
        public List<GameObjectPoolPrefabEntry> MultiPrefabs { get; }
        public int InitialSize { get; }
        public bool CanIncreaseSize { get; }
        public Transform ObjectsContainer { get; }

        public GameObjectPoolBuildArgs(
            string name,
            GameObjectPoolType type,
            GameObject singlePrefab,
            List<GameObjectPoolPrefabEntry> multiPrefabs,
            int initialSize,
            bool canIncreaseSize,
            Transform objectsContainer)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Pool name cannot be null or whitespace.", nameof(name));

            if (initialSize < 0)
                throw new ArgumentOutOfRangeException(nameof(initialSize), "Initial size cannot be negative.");

            Name = name;
            Type = type;
            SinglePrefab = singlePrefab;
            MultiPrefabs = multiPrefabs;
            InitialSize = initialSize;
            CanIncreaseSize = canIncreaseSize;
            ObjectsContainer = objectsContainer;
        }

        private void Validate()
        {
            if (Type == GameObjectPoolType.Single)
            {
                if (SinglePrefab == null)
                    throw new InvalidOperationException("Single prefab must be provided for Single pool type.");

                return;
            }

            if (MultiPrefabs.Count == 0)
                throw new InvalidOperationException("At least one prefab must be provided for Multi pool type.");

            for (int i = 0; i < MultiPrefabs.Count; i++)
            {
                var entry = MultiPrefabs[i];
                if (entry.Prefab == null)
                    throw new InvalidOperationException($"Prefab at index {i} in MultiPrefabs cannot be null.");

                if (entry.Weight < 0)
                    throw new InvalidOperationException($"Weight at index {i} in MultiPrefabs cannot be negative.");
            }
        }
    }
}
