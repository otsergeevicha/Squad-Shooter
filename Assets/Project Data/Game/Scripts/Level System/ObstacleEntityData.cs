using UnityEngine;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class ObstacleEntityData
    {
        public LevelObstaclesType ObstaclesType;

        public Vector3 Position;
        public Quaternion Rotation;

        public ObstacleEntityData(LevelObstaclesType obstaclesType, Vector3 position, Quaternion rotation)
        {
            ObstaclesType = obstaclesType;
            Position = position;
            Rotation = rotation;
        }
    }
}