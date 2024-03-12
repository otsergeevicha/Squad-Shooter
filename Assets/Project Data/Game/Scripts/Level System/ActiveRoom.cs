using System.Collections.Generic;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    public static class ActiveRoom
    {
        private static GameObject levelObject;

        private static RoomData roomData;
        public static RoomData RoomData => roomData;

        private static LevelData levelData;
        public static LevelData LevelData => levelData;

        private static List<GameObject> activeObjects;
        private static int activeObjectsCount;

        private static List<BaseEnemyBehavior> enemies;
        public static List<BaseEnemyBehavior> Enemies => enemies;
        private static int enemiesCount;

        private static List<AbstractChestBehavior> chests;
        public static List<AbstractChestBehavior> Chests => chests;

        private static int currentLevelIndex;
        public static int CurrentLevelIndex => currentLevelIndex;

        private static int currentWorldIndex;
        public static int CurrentWorldIndex => currentWorldIndex;

        private static ExitPointBehaviour exitPointBehaviour;
        public static ExitPointBehaviour ExitPointBehaviour => exitPointBehaviour;

        public static void Initialise(GameObject levelObject)
        {
            ActiveRoom.levelObject = levelObject;

            activeObjects = new List<GameObject>();
            activeObjectsCount = 0;

            enemies = new List<BaseEnemyBehavior>();
            enemiesCount = 0;

            chests = new List<AbstractChestBehavior>();
        }

        public static void SetLevelData(int currentWorldIndex, int currentLevelIndex)
        {
            ActiveRoom.currentWorldIndex = currentWorldIndex;
            ActiveRoom.currentLevelIndex = currentLevelIndex;
        }

        public static void SetLevelData(LevelData levelData)
        {
            ActiveRoom.levelData = levelData;
        }

        public static void SetRoomData(RoomData roomData)
        {
            ActiveRoom.roomData = roomData;
        }

        public static void Unload()
        {
            // Unload created obstacles
            for(int i = 0; i < activeObjectsCount; i++)
            {
                activeObjects[i].transform.SetParent(null);
                activeObjects[i].SetActive(false);
            }

            activeObjects.Clear();
            activeObjectsCount = 0;

            // Unload enemies
            for (int i = 0; i < enemiesCount; i++)
            {
                enemies[i].Unload();

                Object.Destroy(enemies[i].gameObject);
            }

            enemies.Clear();
            enemiesCount = 0;

            if (exitPointBehaviour != null)
            {
                exitPointBehaviour.Unload();

                Object.Destroy(exitPointBehaviour.gameObject);

                exitPointBehaviour = null;
            }
        }

        #region Environment/Obstacles
        public static void SpawnObstacle(LevelObstacle obstacle, ObstacleEntityData obstacleEntityData)
        {
            GameObject obstacleObject = obstacle.Pool.GetPooledObject(false);
            obstacleObject.transform.SetParent(levelObject.transform);
            obstacleObject.transform.SetPositionAndRotation(obstacleEntityData.Position, obstacleEntityData.Rotation);
            obstacleObject.transform.localScale = Vector3.one;
            obstacleObject.SetActive(true);

            activeObjects.Add(obstacleObject);
            activeObjectsCount++;
        }

        public static void SpawnEnvironment(LevelEnvironment environment, EnvironmentEntityData environmentEntityData)
        {
            GameObject obstacleObject = environment.Pool.GetPooledObject(false);
            obstacleObject.transform.SetParent(levelObject.transform);
            obstacleObject.transform.SetPositionAndRotation(environmentEntityData.Position, environmentEntityData.Rotation);
            obstacleObject.transform.localScale = Vector3.one;
            obstacleObject.SetActive(true);

            activeObjects.Add(obstacleObject);
            activeObjectsCount++;
        }

        public static void SpawnExitPoint(GameObject exitPointPrefab, Vector3 position)
        {
            exitPointBehaviour = Object.Instantiate(exitPointPrefab, position, Quaternion.identity, levelObject.transform).GetComponent<ExitPointBehaviour>();
            exitPointBehaviour.Initialise();
        }

        public static void SpawnChest(ChestEntityData chestEntityData, ChestData chestData)
        {
            GameObject chestObject = chestData.Pool.GetPooledObject(false);
            chestObject.transform.SetParent(levelObject.transform);
            chestObject.transform.SetPositionAndRotation(chestEntityData.Position, chestEntityData.Rotation);
            chestObject.transform.localScale = Vector3.one;
            chestObject.SetActive(true);

            chests.Add(chestObject.GetComponent<AbstractChestBehavior>());

            activeObjects.Add(chestObject);
            activeObjectsCount++;
        }

        #endregion

        #region Enemies
        public static BaseEnemyBehavior SpawnEnemy(EnemyData enemyData, EnemyEntityData enemyEntityData, bool isActive)
        {
            BaseEnemyBehavior enemy = Object.Instantiate(enemyData.Prefab, enemyEntityData.Position, enemyEntityData.Rotation, levelObject.transform).GetComponent<BaseEnemyBehavior>();
            enemy.SetEnemyData(enemyData, enemyEntityData.IsElite);
            enemy.SetPatrollingPoints(enemyEntityData.PathPoints);

            // Place enemy on the middle of the path if there are two or more waypoints
            if (enemyEntityData.PathPoints.Length > 1)
                enemy.transform.position = enemyEntityData.PathPoints[0] + (enemyEntityData.PathPoints[1] - enemyEntityData.PathPoints[0]) * 0.5f;

            if(isActive)
                enemy.Initialise();

            enemies.Add(enemy);
            enemiesCount++;

            return enemy;
        }

        public static void ActivateEnemies()
        {
            for (int i = 0; i < enemiesCount; i++)
            {
                enemies[i].Initialise();
            }
        }

        public static void ClearEnemies()
        {
            for (int i = 0; i < enemiesCount; i++)
            {
                enemies[i].Unload();

                Object.Destroy(enemies[i].gameObject);
            }

            enemies.Clear();
            enemiesCount = 0;
        }

        public static BaseEnemyBehavior GetEnemyForSpecialReward()
        {
            BaseEnemyBehavior result = enemies.Find(e => e.Tier == EnemyTier.Boss);

            if (result != null)
                return result;

            result = enemies.Find(e => e.Tier == EnemyTier.Elite);

            if (result != null)
                return result;

            result = enemies[0];

            for (int i = 1; i < enemiesCount; i++)
            {
                if (enemies[i].transform.position.z > result.transform.position.z)
                {
                    result = enemies[i];
                }
            }

            return result;
        }

        public static void InitialiseDrop(List<DropData> enemyDrop, List<DropData> chestDrop)
        {
            for (int i = 0; i < enemiesCount; i++)
            {
                enemies[i].ResetDrop();
            }

            for (int i = 0; i < enemyDrop.Count; i++)
            {
                if (enemyDrop[i].dropType == DropableItemType.Currency && enemyDrop[i].currencyType == CurrencyType.Coin)
                {
                    List<int> coins = LevelController.SplitIntEqually(enemyDrop[i].amount, enemies.Count);

                    for (int j = 0; j < enemies.Count; j++)
                    {
                        enemies[j].AddDrop(new DropData() { dropType = DropableItemType.Currency, currencyType = CurrencyType.Coin, amount = coins[j] });
                    }
                }
                else
                {
                    GetEnemyForSpecialReward().AddDrop(enemyDrop[i]);
                }
            }

            for(int i = 0; i < chests.Count; i++)
            {
                chests[i].Init(chestDrop);
            }
        }

        public static List<BaseEnemyBehavior> GetAliveEnemies()
        {
            List<BaseEnemyBehavior> result = new List<BaseEnemyBehavior>();

            for (int i = 0; i < enemiesCount; i++)
            {
                if (!enemies[i].IsDead)
                {
                    result.Add(enemies[i]);
                }
            }

            return result;
        }

        public static bool AreAllEnemiesDead()
        {
            for (int i = 0; i < enemiesCount; i++)
            {
                if (!enemies[i].IsDead)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}