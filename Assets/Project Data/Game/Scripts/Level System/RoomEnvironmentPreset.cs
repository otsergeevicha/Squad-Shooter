using UnityEngine;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class RoomEnvironmentPreset 
    {
        [SerializeField] RoomEnvironmentPresetType type;
        public RoomEnvironmentPresetType Type => type;

        [SerializeField] EnvironmentEntityData[] environmentEntities;
        public EnvironmentEntityData[] EnvironmentEntities => environmentEntities;

        [SerializeField] Vector3 spawnPos;
        public Vector3 SpawnPos => spawnPos;

        [SerializeField] Vector3 exitPointPos;
        public Vector3 ExitPointPos => exitPointPos;
    }
}