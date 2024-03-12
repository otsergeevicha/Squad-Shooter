using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.SquadShooter
{
    public class BlackFadeBehavior : MonoBehaviour
    {
        [SerializeField] RawImage image;
        [SerializeField] Canvas canvas;
        [SerializeField] CanvasScaler scaler;

        [SerializeField] Gradient gradient;

        private Vector2 size;
        private Vector2 center;

        private bool IsInited { get; set; }

        private void Init()
        {
            float screenWidth;
            float screenHeight;
            if (UIController.IsTablet)
            {
                var height = canvas.pixelRect.height;
                screenHeight = scaler.referenceResolution.y;

                screenWidth = canvas.pixelRect.width / height * screenHeight;
            }
            else
            {
                var width = canvas.pixelRect.width;
                screenWidth = scaler.referenceResolution.x;

                screenHeight = canvas.pixelRect.height / width * screenWidth;
            }

            var start = 0f;
            var end = 1f;

            for (int i = 0; i < gradient.alphaKeys.Length; i++)
            {
                var key = gradient.alphaKeys[i];

                if (key.alpha == 1f)
                {
                    start = key.time;
                    break;
                }
            }

            for (int i = gradient.alphaKeys.Length - 1; i >= 0; i--)
            {
                var key = gradient.alphaKeys[i];

                if (key.alpha == 1f)
                {
                    end = key.time;
                    break;
                }
            }

            var sum = end - start;

            var imageHeight = screenHeight / sum;

            size = new Vector2(screenWidth + 10, imageHeight);
            image.rectTransform.anchoredPosition = new Vector2(0, imageHeight);
            image.rectTransform.sizeDelta = size;

            var texture = new Texture2D(1, 100);
            texture.wrapMode = TextureWrapMode.Clamp;

            var colors = new Color32[100];

            for (int i = 0; i < 100; i++)
            {
                colors[i] = gradient.Evaluate(i / 99f);
            }

            texture.SetPixels32(colors);
            texture.Apply();

            image.texture = texture;

            center = new Vector3(0, ((end + start) / 2 - 0.5f) * size.y);

            gameObject.SetActive(false);
        }

        public void SlideHide(bool disable = true, Tween.TweenCallback onComplete = null)
        {
            if (!IsInited)
                Init();

            gameObject.SetActive(true);

            image.rectTransform.anchoredPosition = new Vector2(0, size.y);
            image.DOAnchoredPosition(center, 0.4f).SetEasing(Ease.Type.Linear).OnComplete(() =>
            {
                if (disable)
                    Disable();
                onComplete?.Invoke();
            });
        }

        public void SlideReveal(bool disable = true, Tween.TweenCallback onComplete = null)
        {
            if (!IsInited)
                Init();

            gameObject.SetActive(true);

            image.rectTransform.anchoredPosition = center;
            image.DOAnchoredPosition(new Vector2(0, -size.y), 0.4f).SetEasing(Ease.Type.Linear).OnComplete(() =>
            {
                if (disable)
                    Disable();
                onComplete?.Invoke();
            });
        }

        public void Hide(bool disable = true, Tween.TweenCallback onComplete = null)
        {
            if (!IsInited)
                Init();

            gameObject.SetActive(true);

            image.rectTransform.anchoredPosition = center;
            image.SetAlpha(0f);
            image.DOFade(1f, 0.4f).OnComplete(() =>
            {
                if (disable)
                    Disable();
                onComplete?.Invoke();
            });
        }

        public void Reveal(bool disable = true, Tween.TweenCallback onComplete = null)
        {
            if (!IsInited)
                Init();

            gameObject.SetActive(true);

            image.rectTransform.anchoredPosition = center;
            image.SetAlpha(1f);
            image.DOFade(0f, 0.4f).OnComplete(() =>
            {
                if (disable)
                    Disable();
                onComplete?.Invoke();
            });
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}