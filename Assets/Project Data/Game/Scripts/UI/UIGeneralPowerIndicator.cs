using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

namespace Watermelon.SquadShooter
{
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public class UIGeneralPowerIndicator : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] Image arrowImage;

        private CanvasGroup canvasGroup;
        private TweenCase fadeTweenCase;

        private static UIGeneralPowerIndicator instance;

        private void Awake()
        {
            instance = this;
            canvasGroup = GetComponent<CanvasGroup>();

            canvasGroup.alpha = 0;
            arrowImage.gameObject.SetActive(false);
        }

        public static void UpdateText(bool highlight = false)
        {
            float delay = highlight ? 0.5f : 0f;

            Tween.DelayedCall(delay, () =>
            {
                instance.text.text = BalanceController.CurrentGeneralPower.ToString();
            });

            if (highlight)
            {
                instance.arrowImage.gameObject.SetActive(true);
                instance.text.transform.DOPushScale(1.3f, 1f, 0.6f, 0.4f, Ease.Type.SineIn, Ease.Type.SineOut).OnComplete(() =>
                {
                    instance.arrowImage.gameObject.SetActive(false);
                });
            }
        }

        public static void Show()
        {
            UpdateText();

            instance.gameObject.SetActive(true);

            if (instance.fadeTweenCase != null && instance.fadeTweenCase.isActive)
                instance.fadeTweenCase.Kill();

            instance.fadeTweenCase = instance.canvasGroup.DOFade(1, 0.3f);
        }

        public static void ShowImmediately()
        {
            UpdateText();

            if (instance.fadeTweenCase != null && instance.fadeTweenCase.isActive)
                instance.fadeTweenCase.Kill();

            instance.gameObject.SetActive(true);

            instance.canvasGroup.alpha = 1.0f;
        }

        public static void Hide()
        {
            if (instance.fadeTweenCase != null && instance.fadeTweenCase.isActive)
                instance.fadeTweenCase.Kill();

            instance.fadeTweenCase = instance.canvasGroup.DOFade(0, 0.3f).OnComplete(() =>
            {
                instance.gameObject.SetActive(false);
            });
        }

        public static void Clear()
        {
            if (instance.fadeTweenCase != null && instance.fadeTweenCase.isActive)
                instance.fadeTweenCase.Kill();
        }
    }
}