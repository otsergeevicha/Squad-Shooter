using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class FlyingTextBehavior : MonoBehaviour
    {
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        [SerializeField] Text text;
        [SerializeField] Image image;

        [SerializeField] AnimationCurve scaleAnimationCurve;

        private int amount;
        public int Amount => amount;

        public int Order { get; set; }

        private float startTime;

        private static float minScale = 1f;
        private static float maxScale = 1.5f;

        private static float maxDuration = 10f;

        private static float minValue = 0.1f;
        private static float maxValue = 0.2f;

        private static float minAnimSpeed = 0.6f;
        private static float maxAnimSpeed = 0.75f;

        private static Ease.IEasingFunction quadIn = Ease.GetFunction(Ease.Type.QuadIn);

        private bool isAlive = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Init(RectTransform parent, Sprite icon, Vector2 position)
        {
            canvasGroup.alpha = 0;

            rectTransform.SetParent(parent);

            rectTransform.localScale = Vector3.one;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.anchoredPosition = position - Vector2.up * 50;

            image.sprite = icon;

            canvasGroup.DOFade(1, 0.2f);
            rectTransform.DOAnchoredPosition(position, 0.5f).SetEasing(Ease.Type.QuadOut);

            amount = 0;

            startTime = Time.time;

            isAlive = true;
        }

        private void Update()
        {
            if (!isAlive) return;

            var time = Time.time - startTime;

            var t = quadIn.Interpolate(Mathf.Clamp01(Mathf.InverseLerp(0, maxDuration, time)));

            var scale = Mathf.Lerp(minScale, maxScale, t);
            var value = Mathf.Lerp(minValue, maxValue, t);

            var speed = Mathf.Lerp(minAnimSpeed, maxAnimSpeed, t);

            var animationTime = scaleAnimationCurve.Evaluate((time * speed) % 1);

            rectTransform.localScale = Vector3.one * (scale + value * animationTime);
        }

        public void UpdateText(int amount)
        {
            this.amount = amount;

            text.text = $"+{this.amount}";
        }

        public void Fly(Vector2 finalPosition, Tween.TweenCallback onComplete = null)
        {
            rectTransform.DOAnchoredPosition(finalPosition, 0.6f).SetEasing(Ease.Type.QuadOutIn).OnComplete(onComplete);
            rectTransform.DOScale(0.5f, 0.6f).SetEasing(Ease.Type.SineOut);

            Tween.DelayedCall(0.4f, () => canvasGroup.DOFade(0, 0.2f));

            isAlive = false;
        }
    }
}

