using UnityEngine;

namespace Watermelon.LevelSystem
{
    [CreateAssetMenu(fileName = "World", menuName = "Content/New Level/World")]
    public class World : ScriptableObject
    {
        [SerializeField] Sprite previewSprite;
        public Sprite PreviewSprite => previewSprite;

        [SerializeField] WorldPresetType obstaclesPresetType;
        public WorldPresetType ObstaclesPresetType => obstaclesPresetType;

        [SerializeField] LevelData[] levels;
        public LevelData[] Levels => levels;

        public void Initialise()
        {
            for (int i = 0; i < levels.Length; i++)
            {
                levels[i].Initialise(this);
            }
        }
    }
}
