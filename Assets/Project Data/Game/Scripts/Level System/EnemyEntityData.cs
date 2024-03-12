using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class EnemyEntityData
    {
        public EnemyType EnemyType;

        public Vector3 Position;
        public Quaternion Rotation;

        public bool IsElite;

        public Vector3[] PathPoints;

        public EnemyEntityData(EnemyType enemyType, Vector3 position, Quaternion rotation, bool isElite, Vector3[] pathPoints)
        {
            EnemyType = enemyType;
            Position = position;
            Rotation = rotation;
            IsElite = isElite;
            PathPoints = pathPoints;
        }
    }
}