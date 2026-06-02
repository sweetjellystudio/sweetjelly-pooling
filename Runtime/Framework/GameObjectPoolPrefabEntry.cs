using UnityEngine;
using System;

namespace SWeetJelly.Pooling.Framework
{
    [Serializable]
    public struct GameObjectPoolPrefabEntry
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int weight;
        [SerializeField] private bool isWeightLocked;

        public readonly GameObject Prefab => prefab;
        public readonly int Weight => weight;
        public readonly bool IsWeightLocked => isWeightLocked;

        public GameObjectPoolPrefabEntry(
            GameObject prefab,
            int weight,
            bool isWeightLocked = false)
        {
            this.prefab = prefab;
            this.weight = weight;
            this.isWeightLocked = isWeightLocked;
        }
    }
}
