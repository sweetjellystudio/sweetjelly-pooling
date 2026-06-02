using System.Collections.Generic;
using UnityEngine;

namespace SWeetJelly.Pooling.Framework
{
    [CreateAssetMenu(fileName = "GameObject Pool Database", menuName = "SWeetJelly/Pooling/GameObject Pool Database")]
    public class GameObjectPoolDatabaseSo : ScriptableObject
    {
        [SerializeField] GameObjectPoolConfig[] pools;
        public IReadOnlyList<GameObjectPoolConfig> Pools => pools;
    }
}
