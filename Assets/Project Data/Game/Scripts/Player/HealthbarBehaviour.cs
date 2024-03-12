using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

namespace Watermelon.SquadShooter
{

    [System.Serializable]
    public class HealthbarBehaviour : MonoBehaviour
    {
        [SerializeField] Transform healthBarTransform;
        public Transform HealthBarTransform => healthBarTransform;

        [SerializeField] Vector3 healthbarOffset;
        public Vector3 HealthbarOffset => healthbarOffset;

        [Space]
        [SerializeField] CanvasGroup healthBarCanvasGroup;
        [SerializeField] Image healthFillImage;
        [SerializeField] Image maskFillImage;
        [SerializeField] TextMeshProUGUI healthText;
        [SerializeField] Image shieldImage;

        [Space]
        [SerializeField] Color standartHealthbarColor;
        [SerializeField] Color specialHealthbarColor;
        [SerializeField] Color standartShieldColor;
        [SerializeField] Color specialShieldColor;

        private IHealth targetHealth;
        private Transform parentTransform;
        private bool showAlways;

        private Vector3 defaultOffset;

        private bool isInitialised;
        private bool isPanelActive;
        private bool isDisabled;
        private int level;

        private TweenCase maskTweenCase;
        private TweenCase panelTweenCase;

        public void Initialise(Transform parentTransform, IHealth targetHealth, bool showAlways, Vector3 defaultOffset, int level = -1, bool isSpecial = false)
        {
            this.targetHealth = targetHealth;
            this.parentTransform = parentTransform;
            this.defaultOffset = defaultOffset;
            this.level = level;
            this.showAlways = showAlways;

            isDisabled = false;
            isPanelActive = false;

            // Reset bar parent
            healthBarTransform.SetParent(null);
            healthBarTransform.gameObject.SetActive(true);

            if (isSpecial)
            {
                healthFillImage.color = specialHealthbarColor;
                shieldImage.color = specialShieldColor;
            }
            else
            {
                healthFillImage.color = standartHealthbarColor;
                shieldImage.color = standartShieldColor;
            }

            // Redraw health
            RedrawHealth();

            // Show or hide healthbar
            healthBarCanvasGroup.alpha = showAlways ? 1.0f : 0.0f;

            isInitialised = true;
        }

        public void FollowUpdate()
        {
            if (isInitialised)
            {
                healthBarTransform.position = parentTransform.position + defaultOffset;
                healthBarTransform.rotation = Camera.main.transform.rotation;
            }
        }

        public void OnHealthChanged()
        {
            if (isDisabled)
                return;

            if (targetHealth == null)
                return;

            healthFillImage.fillAmount = targetHealth.CurrentHealth / targetHealth.MaxHealth;

            if (maskTweenCase != null && !maskTweenCase.isCompleted)
                maskTweenCase.Kill();

            maskTweenCase = maskFillImage.DOFillAmount(healthFillImage.fillAmount, 0.3f).SetEasing(Ease.Type.QuintIn);

            if (level == -1)
            {
                healthText.text = targetHealth.CurrentHealth.ToString("F0");
            }

            if (!showAlways)
            {
                if (healthFillImage.fillAmount < 1.0f && !isPanelActive)
                {
                    isPanelActive = true;

                    if (panelTweenCase != null && !panelTweenCase.isCompleted)
                        panelTweenCase.Kill();

                    panelTweenCase = healthBarCanvasGroup.DOFade(1.0f, 0.5f);
                }
                else if (healthFillImage.fillAmount >= 1.0f && isPanelActive)
                {
                    isPanelActive = false;

                    if (panelTweenCase != null && !panelTweenCase.isCompleted)
                        panelTweenCase.Kill();

                    panelTweenCase = healthBarCanvasGroup.DOFade(0.0f, 0.5f);
                }
            }
        }

        public void DisableBar()
        {
            if (isDisabled)
                return;

            isDisabled = true;

            healthBarCanvasGroup.DOFade(0.0f, 0.3f).OnComplete(delegate
            {
                healthBarTransform.gameObject.SetActive(false);
            });
        }

        public void EnableBar()
        {
            if (!isDisabled)
                return;

            isDisabled = false;

            healthBarTransform.gameObject.SetActive(true);
            healthBarCanvasGroup.DOFade(1.0f, 0.3f);
        }

        public void RedrawHealth()
        {
            healthFillImage.fillAmount = targetHealth.CurrentHealth / targetHealth.MaxHealth;
            maskFillImage.fillAmount = healthFillImage.fillAmount;

            if (level == -1)
            {
                shieldImage.gameObject.SetActive(false);
                healthText.text = targetHealth.CurrentHealth.ToString("F0");
            }
            else
            {
                shieldImage.gameObject.SetActive(true);
                healthText.text = level.ToString();
            }
        }

        public void ForceDisable()
        {
            isDisabled = true;

            healthBarTransform.gameObject.SetActive(false);
            healthBarCanvasGroup.gameObject.SetActive(false);
        }

        public void Destroy()
        {
            isDisabled = true;

            MonoBehaviour.Destroy(healthBarTransform.gameObject);
        }
    }

    public interface IHealth
    {
        float CurrentHealth { get; }
        float MaxHealth { get; }
    }
}