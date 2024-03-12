using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class LevelTypeSettings
    {
        [SerializeField] LevelType levelType;
        public LevelType LevelType => levelType;

        [SerializeField] GameObject previewObject;

        private Pool previewPool;
        public Pool PreviewPool => previewPool;

        public void Initialise()
        {
            previewPool = new Pool(new PoolSettings(previewObject.name, previewObject, 0, true));
        }
    }
}