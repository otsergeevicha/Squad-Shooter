using UnityEngine;

namespace Watermelon.LevelSystem
{
    [System.Serializable]
    public class WorldPreset
    {
        [SerializeField] WorldPresetType type;
        public WorldPresetType Type => type;

        [SerializeField] LevelObstacle[] obstacles;
        public LevelObstacle[] Obstacles => obstacles;

        [SerializeField] LevelEnvironment[] environments;
        public LevelEnvironment[] Environments => environments;

        [SerializeField] GameObject pedestalPrefab;
        public GameObject PedestalPrefab => pedestalPrefab;

        public void Initialise()
        {
            for(int i = 0; i < obstacles.Length; i++)
            {
                obstacles[i].Inititalise((LevelObstaclesType)i);
            }

            for (int i = 0; i < environments.Length; i++)
            {
                environments[i].Inititalise((LevelEnvironmentType)i);
            }
        }

        public LevelObstacle GetObstacle(LevelObstaclesType levelObstaclesType)
        {
            return obstacles[(int)levelObstaclesType];
        }

        public LevelEnvironment GetEnvironment(LevelEnvironmentType levelEnvironmentType)
        {
            return environments[(int)levelEnvironmentType];
        }

        public void OnPresetLoaded()
        {
            for(int i = 0; i < obstacles.Length; i++)
            {
                obstacles[i].OnPresetLoaded();
            }

            for (int i = 0; i < environments.Length; i++)
            {
                environments[i].OnPresetLoaded();
            }
        }

        public void OnPresetUnloaded()
        {
            for (int i = 0; i < obstacles.Length; i++)
            {
                obstacles[i].OnPresetUnloaded();
            }

            for (int i = 0; i < environments.Length; i++)
            {
                environments[i].OnPresetUnloaded();
            }
        }
    }
}
