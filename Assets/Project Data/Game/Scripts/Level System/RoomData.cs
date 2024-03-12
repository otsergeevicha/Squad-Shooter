using UnityEngine;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class RoomData
    {
        [SerializeField] Vector3 spawnPoint;
        public Vector3 SpawnPoint => spawnPoint;

        [SerializeField] Vector3 exitPoint;
        public Vector3 ExitPoint => exitPoint;

        [Space]
        [SerializeField] EnemyEntityData[] enemyEntities;
        public EnemyEntityData[] EnemyEntities => enemyEntities;

        [SerializeField] ObstacleEntityData[] obstacleEntities;
        public ObstacleEntityData[] ObstacleEntities => obstacleEntities;

        [SerializeField] EnvironmentEntityData[] environmentEntities;
        public EnvironmentEntityData[] EnvironmentEntities => environmentEntities;

        [SerializeField] ChestEntityData[] chestEntities;
        public ChestEntityData[] ChestEntities => chestEntities;
    }
}