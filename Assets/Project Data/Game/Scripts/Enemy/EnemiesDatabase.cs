using UnityEngine;

namespace Watermelon.SquadShooter
{
    [CreateAssetMenu(fileName = "Enemies Database", menuName = "Content/Enemies Database")]
    public class EnemiesDatabase : ScriptableObject
    {
        [SerializeField] EnemyData[] enemies;
        public EnemyData[] Enemies => enemies;

        public void InitialiseStatsRealation(int baseCharacterHealth)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i].Stats.InitialiseStatsRelation(baseCharacterHealth);
            }
        }

        public void SetCurrentCharacterStats(int characterHealth, int weaponDmg)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i].Stats.SetCurrentCreatureStats(characterHealth, weaponDmg, BalanceController.GetActiveDifficultySettings());
            }
        }

        public EnemyData GetEnemyData(EnemyType type)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].EnemyType.Equals(type))
                    return enemies[i];
            }

            Debug.LogError("[Enemies Database] Enemy of type " + type + " + is not found!");
            return enemies[0];
        }
    }
}