using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.SquadShooter
{
    public class PreviewCase
    {
        private GameObject gameObject;
        private LevelTypeSettings levelTypeSettings;

        private RectTransform rectTransform;
        public RectTransform RectTransform => rectTransform;

        private LevelPreviewBaseBehaviour previewBehaviour;
        public LevelPreviewBaseBehaviour PreviewBehaviour => previewBehaviour;

        public PreviewCase(GameObject gameObject, LevelTypeSettings levelTypeSettings)
        {
            this.gameObject = gameObject;
            this.levelTypeSettings = levelTypeSettings;

            rectTransform = (RectTransform)gameObject.transform;

            previewBehaviour = gameObject.GetComponent<LevelPreviewBaseBehaviour>();
            previewBehaviour.Initialise();
        }

        public void Reset()
        {
            gameObject.SetActive(false);
        }
    }
}
