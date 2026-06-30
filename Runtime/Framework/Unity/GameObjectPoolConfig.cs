namespace SWeetJelly.Pooling.Framework.Unity
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using SWeetJelly.Pooling.Contract;

    [Serializable]
    public sealed class GameObjectPoolConfig
    {
        [SerializeField] private string poolId;
        [SerializeField] private GameObjectPoolType poolType = GameObjectPoolType.Single;

        [SerializeField] private GameObject singlePoolPrefab;
        [SerializeField] private List<GameObjectPoolPrefabEntry> multiPoolPrefabs = new();

        [SerializeField] private int initialSize = 10;
        [SerializeField] private bool canIncreaseSize = true;
        [SerializeField] private Transform objectsContainer;

        public PoolId PoolId => new PoolId(poolId);
        public string PoolName => poolId;
        public GameObjectPoolType PoolType => poolType;

        public GameObject SinglePoolPrefab => singlePoolPrefab;
        public IReadOnlyList<GameObjectPoolPrefabEntry> MultiPoolPrefabs => multiPoolPrefabs;

        public int InitialSize => initialSize;
        public bool CanIncreaseSize => canIncreaseSize;
        public Transform ObjectsContainer => objectsContainer;

        public GameObjectPoolBuildArgs ToBuildArgs(Transform defaultContainer = null)
        {
            Transform container = objectsContainer != null
                ? objectsContainer
                : defaultContainer;

            return new GameObjectPoolBuildArgs(
                poolId,
                poolType,
                singlePoolPrefab,
                multiPoolPrefabs,
                initialSize,
                canIncreaseSize,
                container);
        }
    }
}