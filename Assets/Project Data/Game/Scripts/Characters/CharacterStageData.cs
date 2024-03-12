using UnityEngine;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class CharacterStageData
    {
        [SerializeField] Sprite previewSprite;
        public Sprite PreviewSprite => previewSprite;

        [SerializeField] Sprite lockedSprite;
        public Sprite LockedSprite => lockedSprite;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] Vector3 healthBarOffset;
        public Vector3 HealthBarOffset => healthBarOffset;
    }
}