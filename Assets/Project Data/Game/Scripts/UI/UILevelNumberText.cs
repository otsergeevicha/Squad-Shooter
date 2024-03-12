using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    [RequireComponent(typeof(Text))]
    public class UILevelNumberText : MonoBehaviour
    {
        private const string LEVEL_LABEL = "LEVEL {0}";
        private static UILevelNumberText instance;

        [SerializeField] UIScalableObject uIScalableObject;

        private static UIScalableObject UIScalableObject => instance.uIScalableObject;
        private static Text levelNumberText;

        private static bool IsDisplayed = false;

        private void Awake()
        {
            instance = this;
            levelNumberText = GetComponent<Text>();
        }

        private void Start()
        {
            UpdateLevelNumber();
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        public static void Show(bool immediately = true)
        {
            if (IsDisplayed) return;

            IsDisplayed = true;

            levelNumberText.enabled = true;
            UIScalableObject.Show(immediately, scaleMultiplier: 1.05f);
        }

        public static void Hide(bool immediately = true)
        {
            if (!IsDisplayed) return;

            if (immediately) IsDisplayed = false;

            UIScalableObject.Hide(immediately, scaleMultiplier: 1.05f, onCompleted: delegate {

                IsDisplayed = false;
                levelNumberText.enabled = false;
            });
        }

        private void UpdateLevelNumber()
        {
            levelNumberText.text = string.Format(LEVEL_LABEL, "X");
        }

    }
}
