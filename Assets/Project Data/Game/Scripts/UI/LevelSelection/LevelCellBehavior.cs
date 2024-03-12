#pragma warning disable 0649

using UnityEngine;
using UnityEngine.UI;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class LevelCellBehavior : MonoBehaviour, GridItem
    {
        [SerializeField] Text levelNumber;
        [SerializeField] Image currentLevelIndicator;
        [SerializeField] Image lockImage;

        [Space]
        [SerializeField] Button button;

        private RectTransform rectTransform;

        private int levelId;
        public int LevelNumber {
            get => levelId;
            set {
                levelNumber.text = (value + 1).ToString();
                levelId = value;
            }
        }

        public bool IsSelected { set => currentLevelIndicator.enabled = value; }

        public bool IsOpened { 
            set
            {
                lockImage.enabled = !value;
                levelNumber.enabled = value;

                button.enabled = value;
            } 
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public RectTransform GetRectTransform()
        {
            return rectTransform;
        }

        public void InitGridItem(int id)
        {

        }

        public void OnClick()
        {
            Tween.DelayedCall(0.4f, () => {
                UIController.HidePage<UIGame>();
            });
        }
    }
}

