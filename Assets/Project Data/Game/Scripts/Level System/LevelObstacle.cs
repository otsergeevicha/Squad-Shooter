using UnityEngine;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class LevelObstacle
    {
        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] int recommendedCount;
        public int RecommendedCount => recommendedCount;

        private LevelObstaclesType type;
        public LevelObstaclesType Type => type;

        private Pool pool;
        public Pool Pool => pool;

        public void Inititalise(LevelObstaclesType type)
        {
            this.type = type;
        }

        public void OnPresetLoaded()
        {
            pool = new Pool(new PoolSettings(prefab.name, prefab, 0, true));
        }

        public void OnPresetUnloaded()
        {
            pool.Clear();
            pool = null;
        }

        public void RemoveUnusedCopies()
        {
            // Destoy elements if count > recommendedCount
        }
    }
}