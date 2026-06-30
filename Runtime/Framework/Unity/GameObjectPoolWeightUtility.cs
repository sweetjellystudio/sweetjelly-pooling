using System.Collections.Generic;

namespace SWeetJelly.Pooling.Framework.Unity
{
    public static class GameObjectPoolWeightUtility
    {
        public static List<GameObjectPoolPrefabEntry> Recalculate(
            IReadOnlyList<GameObjectPoolPrefabEntry> entries)
        {
            var result = new List<GameObjectPoolPrefabEntry>();

            if (entries == null || entries.Count == 0)
                return result;

            int totalUnlockedPoints = 100;
            int unlockedCount = entries.Count;

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].IsWeightLocked)
                {
                    totalUnlockedPoints -= entries[i].Weight;
                    unlockedCount--;
                }
            }

            if (unlockedCount <= 0)
            {
                result.AddRange(entries);
                return result;
            }

            int average = totalUnlockedPoints / unlockedCount;
            int remainder = totalUnlockedPoints - average * unlockedCount;

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                if (entry.IsWeightLocked)
                {
                    result.Add(entry);
                    continue;
                }

                int weight = average + (remainder > 0 ? 1 : 0);
                remainder--;

                result.Add(new GameObjectPoolPrefabEntry(
                    entry.Prefab,
                    weight,
                    false));
            }

            return result;
        }
    }
}