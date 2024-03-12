using UnityEngine;
using UnityEngine.UI;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class OffscreenEnemyIndicator : MonoBehaviour
    {
        [SerializeField] Image image;
        private RectTransform rect;

        private BaseEnemyBehavior enemy;

        private TweenCase fadeCase;

        private bool IsEnabled { get; set; }
        private bool IsTransparent { get; set; }

        private Vector2 screenSize;

        private Vector2 centerViewportPos;

        private Vector2 parentViewportMin;
        private Vector2 parentViewportMax;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();

        }

        private void OnEnable()
        {
            image.SetAlpha(0f);
        }

        public void Init(BaseEnemyBehavior enemy, RectTransform parent, RectTransform baseCanvasRect)
        {
            screenSize = baseCanvasRect.rect.size;//Camera.main.pixelRect.size;
            var pixelViewportSize = new Vector2(1f / screenSize.x, 1f / screenSize.y);

            float minX = parent.offsetMin.x;
            float maxX = screenSize.x + parent.offsetMax.x;

            float minY = parent.offsetMin.y;
            float maxY = screenSize.y + parent.offsetMax.y;

            float centerX = (minX + maxX) / 2f;
            float centerY = (minY + maxY) / 2f;

            centerViewportPos = new Vector2(centerX * pixelViewportSize.x, centerY * pixelViewportSize.y);

            parentViewportMin = new Vector2(minX * pixelViewportSize.x, minY * pixelViewportSize.y);
            parentViewportMax = new Vector2(maxX * pixelViewportSize.x, maxY * pixelViewportSize.y);

            this.enemy = enemy;
            IsEnabled = enemy.IsVisible;

            if (!IsEnabled)
            {
                Show();
            }
        }

        public void Show()
        {
            fadeCase.KillActive();
            fadeCase = image.DOFade(1, 0.3f);

            IsTransparent = false;
        }

        public void Hide()
        {
            fadeCase.KillActive();
            fadeCase = image.DOFade(0, 0.3f).OnComplete(() => IsTransparent = true);
        }

        private void Update()
        {
            if (IsEnabled)
            {
                if (!enemy.IsVisible)
                {
                    IsEnabled = false;
                    Show();
                }
            }
            else
            {
                if (enemy.IsVisible)
                {
                    IsEnabled = true;
                    Hide();
                }
            }

            if (!IsTransparent)
            {
                var enemyScreenPosition = Camera.main.WorldToScreenPoint(enemy.transform.position);
                var enemyViewportPosition = Camera.main.ScreenToViewportPoint(enemyScreenPosition);

                if (enemyViewportPosition.x <= 0)
                {
                    if (enemyViewportPosition.y <= 0)
                    {
                        rect.localRotation = Quaternion.Euler(0, 0, -135);
                    }
                    else if (enemyViewportPosition.y >= 1)
                    {
                        rect.localRotation = Quaternion.Euler(0, 0, 135);
                    }
                    else
                    {
                        rect.localRotation = Quaternion.Euler(0, 0, 180);
                    }
                }
                else if (enemyViewportPosition.x >= 1)
                {
                    if (enemyViewportPosition.y <= 0)
                    {
                        rect.localRotation = Quaternion.Euler(0, 0, -45);
                    }
                    else if (enemyViewportPosition.y >= 1)
                    {
                        rect.localRotation = Quaternion.Euler(0, 0, 45);
                    }
                    else
                    {
                        rect.localRotation = Quaternion.Euler(0, 0, 0);
                    }
                }
                else
                {
                    if (enemyViewportPosition.y <= 0)
                    {
                        rect.localRotation = Quaternion.Euler(0, 0, -90);
                    }
                    else if (enemyViewportPosition.y >= 1)
                    {
                        rect.localRotation = Quaternion.Euler(0, 0, 90);
                    }
                }

                enemyViewportPosition.x = Mathf.Clamp(enemyViewportPosition.x, parentViewportMin.x, parentViewportMax.x);
                enemyViewportPosition.y = Mathf.Clamp(enemyViewportPosition.y, parentViewportMin.y, parentViewportMax.y);

                var anchoredPos = new Vector2((enemyViewportPosition.x - parentViewportMin.x) * screenSize.x, (enemyViewportPosition.y - parentViewportMin.y) * screenSize.y);
                rect.anchoredPosition = anchoredPos;
            }
        }

        private Vector2 MoveXToN(Vector2 viewportPos, float n)
        {
            var direction = (viewportPos - centerViewportPos).normalized;

            var k = (n - centerViewportPos.x) / direction.x;
            var y = centerViewportPos.y + k * direction.y;

            return new Vector2(n, y);
        }


        private Vector2 MoveYToN(Vector2 viewportPos, float n)
        {
            var direction = (viewportPos - centerViewportPos).normalized;

            var k = (1f - centerViewportPos.y) / direction.y;
            var x = centerViewportPos.x + k * direction.x;

            return new Vector2(x, n);
        }
    }
}