using UnityEngine;
using System;
using System.Collections.Generic;

namespace SWeetJelly.Pooling.Framework.Unity
{
    public enum GameObjectPoolType
    {
        Single = 0,
        Multi = 1,
    }

    public sealed class GameObjectPool
    {
        private readonly string name;
        private readonly GameObjectPoolType type;
        private readonly GameObject singlePrefab;
        private readonly IReadOnlyList<GameObjectPoolPrefabEntry> multiPrefabs;
        private readonly int initialSize;
        private readonly bool canIncreaseSize;
        private readonly Transform objectsContainer;

        private readonly List<GameObject> pooledObjects = new List<GameObject>();
        private readonly List<List<GameObject>> multiPooledObjects = new List<List<GameObject>>();

        private bool isInitialised;
        private int spawnedCount;

        public string Name => name;
        public GameObjectPoolType Type => type;
        public GameObject SinglePrefab => singlePrefab;
        public IReadOnlyList<GameObjectPoolPrefabEntry> MultiPrefabs => multiPrefabs;
        public int InitialSize => initialSize;
        public bool CanIncreaseSize => canIncreaseSize;
        public Transform ObjectsContainer => objectsContainer;
        public bool IsInitialised => isInitialised;

        public GameObjectPool(GameObjectPoolBuildArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            name = args.Name;
            type = args.Type;
            singlePrefab = args.SinglePrefab;
            multiPrefabs = args.MultiPrefabs;
            initialSize = args.InitialSize;
            canIncreaseSize = args.CanIncreaseSize;
            objectsContainer = args.ObjectsContainer;
        }

        public void Initialise()
        {
            if (isInitialised)
                return;

            if (type == GameObjectPoolType.Single)
                InitialiseSinglePool();
            else
                InitialiseMultiPool();

            isInitialised = true;
        }

        public GameObject GetPooledObject(bool activateObject = true)
        {
            var settings = new PooledGameObjectSettings(activateObject, false);
            return GetPooledObject(settings);
        }

        public GameObject GetPooledObject(Vector3 position, bool activateObject = true)
        {
            var settings = new PooledGameObjectSettings(activateObject, false)
                .SetPosition(position);

            return GetPooledObject(settings);
        }

        public GameObject GetPooledObject(PooledGameObjectSettings settings)
        {
            EnsureInitialised();

            if (type == GameObjectPoolType.Single)
                return GetSingle(settings);

            return GetMulti(settings, -1);
        }

        public GameObject GetMultiPooledObjectByIndex(int index, PooledGameObjectSettings settings)
        {
            EnsureInitialised();

            if (type != GameObjectPoolType.Multi)
                throw new InvalidOperationException($"Pool '{name}' is not a multi pool.");

            if (index < 0 || index >= multiPrefabs.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return GetMulti(settings, index);
        }

        public GameObjectPoolPrefabEntry MultiPoolPrefabByIndex(int index)
        {
            if (type != GameObjectPoolType.Multi)
                throw new InvalidOperationException($"Pool '{name}' is not a multi pool.");

            if (index < 0 || index >= multiPrefabs.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return multiPrefabs[index];
        }

        public void CreatePoolObjects(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            EnsureInitialised();

            if (type != GameObjectPoolType.Single)
                throw new InvalidOperationException("CreatePoolObjects currently supports only single pool.");

            int sizeDifference = count - pooledObjects.Count;

            for (int i = 0; i < sizeDifference; i++)
                CreateSingleInstance();
        }

        public void ReturnToPoolEverything(bool resetParent = false)
        {
            EnsureInitialised();

            if (type == GameObjectPoolType.Single)
            {
                for (int i = 0; i < pooledObjects.Count; i++)
                    ReturnObject(pooledObjects[i], resetParent);

                return;
            }

            for (int i = 0; i < multiPooledObjects.Count; i++)
            {
                List<GameObject> objects = multiPooledObjects[i];

                for (int j = 0; j < objects.Count; j++)
                    ReturnObject(objects[j], resetParent);
            }
        }

        public void ResetParent()
        {
            EnsureInitialised();

            if (type == GameObjectPoolType.Single)
            {
                for (int i = 0; i < pooledObjects.Count; i++)
                    ResetParent(pooledObjects[i]);

                return;
            }

            for (int i = 0; i < multiPooledObjects.Count; i++)
            {
                List<GameObject> objects = multiPooledObjects[i];

                for (int j = 0; j < objects.Count; j++)
                    ResetParent(objects[j]);
            }
        }

        public void Clear()
        {
            if (type == GameObjectPoolType.Single)
            {
                for (int i = 0; i < pooledObjects.Count; i++)
                    DestroyObject(pooledObjects[i]);

                pooledObjects.Clear();
            }
            else
            {
                for (int i = 0; i < multiPooledObjects.Count; i++)
                {
                    List<GameObject> objects = multiPooledObjects[i];

                    for (int j = 0; j < objects.Count; j++)
                        DestroyObject(objects[j]);

                    objects.Clear();
                }

                multiPooledObjects.Clear();
            }

            isInitialised = false;
            spawnedCount = 0;
        }

        public bool IsAllPrefabsAssigned()
        {
            if (type == GameObjectPoolType.Single)
                return singlePrefab != null;

            if (multiPrefabs == null || multiPrefabs.Count == 0)
                return false;

            for (int i = 0; i < multiPrefabs.Count; i++)
            {
                if (multiPrefabs[i].Prefab == null)
                    return false;
            }

            return true;
        }

        public void DeleteAllNullRefsInSpawnedObjects()
        {
            if (type == GameObjectPoolType.Single)
            {
                RemoveNullRefs(pooledObjects);
                return;
            }

            for (int i = 0; i < multiPooledObjects.Count; i++)
                RemoveNullRefs(multiPooledObjects[i]);
        }

        private void InitialiseSinglePool()
        {
            pooledObjects.Clear();

            for (int i = 0; i < initialSize; i++)
                CreateSingleInstance();
        }

        private void InitialiseMultiPool()
        {
            multiPooledObjects.Clear();

            for (int i = 0; i < multiPrefabs.Count; i++)
            {
                multiPooledObjects.Add(new List<GameObject>());

                for (int j = 0; j < initialSize; j++)
                    CreateMultiInstance(i);
            }
        }

        private GameObject GetSingle(PooledGameObjectSettings settings)
        {
            for (int i = 0; i < pooledObjects.Count; i++)
            {
                GameObject instance = pooledObjects[i];

                if (instance == null)
                {
                    instance = CreateSingleInstance();
                    pooledObjects[i] = instance;
                }

                if (IsAvailable(instance, settings))
                {
                    SetupPooledObject(instance, settings);
                    return instance;
                }
            }

            if (!canIncreaseSize)
                return null;

            GameObject newInstance = CreateSingleInstance();
            SetupPooledObject(newInstance, settings);
            return newInstance;
        }

        private GameObject GetMulti(PooledGameObjectSettings settings, int explicitPoolIndex)
        {
            int chosenPoolIndex = explicitPoolIndex >= 0
                ? explicitPoolIndex
                : ChooseWeightedPrefabIndex();

            List<GameObject> objects = multiPooledObjects[chosenPoolIndex];

            for (int i = 0; i < objects.Count; i++)
            {
                GameObject instance = objects[i];

                if (instance == null)
                {
                    instance = CreateMultiInstance(chosenPoolIndex);
                    objects[i] = instance;
                }

                if (IsAvailable(instance, settings))
                {
                    SetupPooledObject(instance, settings);
                    return instance;
                }
            }

            if (!canIncreaseSize)
                return null;

            GameObject newInstance = CreateMultiInstance(chosenPoolIndex);
            SetupPooledObject(newInstance, settings);
            return newInstance;
        }

        private GameObject CreateSingleInstance()
        {
            return CreateInstance(singlePrefab, pooledObjects);
        }

        private GameObject CreateMultiInstance(int poolIndex)
        {
            GameObject prefab = multiPrefabs[poolIndex].Prefab;
            return CreateInstance(prefab, multiPooledObjects[poolIndex]);
        }

        private GameObject CreateInstance(GameObject prefab, List<GameObject> list)
        {
            if (prefab == null)
                throw new InvalidOperationException($"Pool '{name}' has a missing prefab.");

            GameObject instance = UnityEngine.Object.Instantiate(prefab, objectsContainer);
            instance.name = $"{prefab.name} {spawnedCount}";
            instance.SetActive(false);

            spawnedCount++;
            list.Add(instance);

            return instance;
        }

        private int ChooseWeightedPrefabIndex()
        {
            int totalWeight = 0;

            for (int i = 0; i < multiPrefabs.Count; i++)
                totalWeight += multiPrefabs[i].Weight;

            if (totalWeight <= 0)
                throw new InvalidOperationException($"Pool '{name}' has invalid total weight.");

            int randomValue = UnityEngine.Random.Range(1, totalWeight + 1);
            int currentValue = 0;

            for (int i = 0; i < multiPrefabs.Count; i++)
            {
                currentValue += multiPrefabs[i].Weight;

                if (randomValue <= currentValue)
                    return i;
            }

            return multiPrefabs.Count - 1;
        }

        private static bool IsAvailable(GameObject instance, PooledGameObjectSettings settings)
        {
            if (instance == null)
                return false;

            return settings.UseActiveOnHierarchy
                ? !instance.activeInHierarchy
                : !instance.activeSelf;
        }

        private void SetupPooledObject(GameObject instance, PooledGameObjectSettings settings)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            Transform objectTransform = instance.transform;

            if (settings.ApplyParent)
                objectTransform.SetParent(settings.Parent);

            if (settings.ApplyPosition)
                objectTransform.position = settings.Position;

            if (settings.ApplyLocalPosition)
                objectTransform.localPosition = settings.LocalPosition;

            if (settings.ApplyEulerRotation)
                objectTransform.eulerAngles = settings.EulerRotation;

            if (settings.ApplyLocalEulerRotation)
                objectTransform.localEulerAngles = settings.LocalEulerRotation;

            if (settings.ApplyRotation)
                objectTransform.rotation = settings.Rotation;

            if (settings.ApplyLocalRotation)
                objectTransform.localRotation = settings.LocalRotation;

            if (settings.ApplyLocalScale)
                objectTransform.localScale = settings.LocalScale;

            instance.SetActive(settings.Activate);
        }

        private void ReturnObject(GameObject instance, bool resetParent)
        {
            if (instance == null)
                return;

            if (resetParent)
                ResetParent(instance);

            instance.SetActive(false);
        }

        private void ResetParent(GameObject instance)
        {
            if (instance == null)
                return;

            instance.transform.SetParent(objectsContainer, false);
        }

        private static void DestroyObject(GameObject instance)
        {
            if (instance == null)
                return;

            UnityEngine.Object.Destroy(instance);
        }

        private static void RemoveNullRefs(List<GameObject> objects)
        {
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                if (objects[i] == null)
                    objects.RemoveAt(i);
            }
        }

        private void EnsureInitialised()
        {
            if (!isInitialised)
                Initialise();
        }
    }
}
