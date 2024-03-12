using UnityEngine;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class ChestData
    {
        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] LevelChestType type;
        public LevelChestType Type => type;

        private Pool pool;
        public Pool Pool => pool;

        public void Initialise()
        {
            pool = PoolManager.AddPool(new PoolSettings(prefab.name, prefab, 1, true));
        }
    }
}