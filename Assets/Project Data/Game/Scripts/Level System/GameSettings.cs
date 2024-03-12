using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    [CreateAssetMenu(fileName = "Game Settings", menuName = "Content/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] LevelsDatabase levelsDatabase;
        public LevelsDatabase LevelsDatabase => levelsDatabase;

        [LineSpacer("Player")]
        [SerializeField] GameObject playerPrefab;
        public GameObject PlayerPrefab => playerPrefab;

        [LineSpacer("Environment")]
        [SerializeField] NavMeshData navMeshData;
        public NavMeshData NavMeshData => navMeshData;

        [SerializeField] GameObject backWallCollider;
        public GameObject BackWallCollider => backWallCollider;

        [Space(5f)]
        [SerializeField] WorldPreset[] worldPresets;
        public WorldPreset[] WorldPresets => worldPresets;

        [SerializeField] GameObject exitPointPrefab;
        public GameObject ExitPointPrefab => exitPointPrefab;

        [SerializeField] RoomEnvironmentPreset[] roomEnvPresets;
        public RoomEnvironmentPreset[] RoomEnvPresets => roomEnvPresets;

        [Space(5f)]
        [SerializeField] ChestData[] chestData;
        public ChestData[] ChestData => chestData;

        [Space]
        [SerializeField] Vector3 pedestalPosition;
        public Vector3 PedestalPosition => pedestalPosition;

        [LineSpacer("Drop")]
        [SerializeField] DropableItemSettings dropSettings;

        [LineSpacer("Minimap")]
        [SerializeField] LevelTypeSettings[] levelTypes;

        [SerializeField] Sprite defaultWorldSprite;
        public Sprite DefaultWorldSprite => defaultWorldSprite;

        private Dictionary<LevelType, int> levelTypesLink;

        public void Initialise()
        {
            levelTypesLink = new Dictionary<LevelType, int>();
            for (int i = 0; i < levelTypes.Length; i++)
            {
                if (!levelTypesLink.ContainsKey(levelTypes[i].LevelType))
                {
                    levelTypes[i].Initialise();

                    levelTypesLink.Add(levelTypes[i].LevelType, i);
                }
                else
                {
                    Debug.LogError(string.Format("[Levels]: Duplicate is found - {0}", levelTypes[i].LevelType));
                }
            }
            
            Drop.Initialise(dropSettings);

            for (int i = 0; i < worldPresets.Length; i++)
            {
                worldPresets[i].Initialise();
            }

            for(int i = 0; i < chestData.Length; i++)
            {
                chestData[i].Initialise();
            }
            
        }

        public LevelTypeSettings GetLevelSettings(LevelType levelType)
        {
            if (levelTypesLink.ContainsKey(levelType))
                return levelTypes[levelTypesLink[levelType]];

            Debug.LogError(string.Format("[Levels]: Level with type '{0}' is missing", levelType));

            return null;
        }

        public WorldPreset GetObstaclesPreset(WorldPresetType presetType)
        {
            for (int i = 0; i < worldPresets.Length; i++)
            {
                if (worldPresets[i].Type == presetType)
                    return worldPresets[i];
            }

            Debug.LogError(string.Format("[Level]: Obstacles preset with type {0} is missing!", presetType));

            return null;
        }

        public ChestData GetChestData(LevelChestType chestType)
        {
            for(int i = 0; i < chestData.Length; i++)
            {
                var data = chestData[i];
                if(chestType == data.Type)
                {
                    return data;
                }
            }

            Debug.LogError(string.Format("[Level]: Chest preset with type {0} is missing!", chestType));

            return null;
        }
    }
}