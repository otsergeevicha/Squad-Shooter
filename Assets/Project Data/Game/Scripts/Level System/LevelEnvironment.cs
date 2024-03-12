using UnityEngine;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class LevelEnvironment
    {
        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] int recommendedCount;
        public int RecommendedCount => recommendedCount;

        private LevelEnvironmentType type;
        public LevelEnvironmentType Type => type;

        private Pool pool;
        public Pool Pool => pool;

        public void Inititalise(LevelEnvironmentType type)
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