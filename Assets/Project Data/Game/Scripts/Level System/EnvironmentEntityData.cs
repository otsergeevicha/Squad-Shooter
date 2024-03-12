using UnityEngine;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class EnvironmentEntityData
    {
        public LevelEnvironmentType EnvironmentType;

        public Vector3 Position;
        public Quaternion Rotation;

        public EnvironmentEntityData(LevelEnvironmentType environmentType, Vector3 position, Quaternion rotation)
        {
            EnvironmentType = environmentType;
            Position = position;
            Rotation = rotation;
        }
    }
}