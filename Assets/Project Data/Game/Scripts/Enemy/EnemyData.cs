using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class EnemyData
    {
        [SerializeField] EnemyType enemyType;
        public EnemyType EnemyType => enemyType;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] EnemyStats stats;
        public EnemyStats Stats => stats;

        [Header("Editor")]
        [SerializeField] Texture2D icon;
        public Texture2D Icon => icon;

        [SerializeField] Color iconTint;
        public Color IconTint => iconTint;
    }
}