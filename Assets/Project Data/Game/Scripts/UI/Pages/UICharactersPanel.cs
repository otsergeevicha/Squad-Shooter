using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class UICharactersPanel : UIPage
    {
        private const float SCROLL_SIDE_OFFSET = 50;
        private const float SCROLL_ELEMENT_WIDTH = 415f;

        [SerializeField] RectTransform backgroundPanelRectTransform;
        [SerializeField] RectTransform closeButtonRectTransform;
        [SerializeField] ScrollRect scrollView;

        [Space]
        [SerializeField] AnimationCurve panelScaleAnimationCurve;
        [SerializeField] AnimationCurve selectedPanelScaleAnimationCurve;

        [Space]
        [SerializeField] GameObject panelUIPrefab;
        [SerializeField] Transform panelsContainer;

        [Space]
        [SerializeField] GameObject stageStarPrefab;

        private CharactersDatabase charactersDatabase;

        private CharacterPanelUI[] characterPanels;

        private bool isAnimationPlaying;
        private Coroutine animationCoroutine;

        private Pool stageStarPool;

        private static bool isControlBlocked = false;
        public static bool IsControlBlocked => isControlBlocked;

        private CustomEasingFunction bounceEasingFunction;

        private static List<CharacterDynamicAnimation> characterDynamicAnimations;

        private Currency[] currencies;

        public override void Initialise()
        {
            charactersDatabase = CharactersController.GetDatabase();

            bounceEasingFunction = Ease.GetCustomEasingFunction("BackOutLight");

            stageStarPool = new Pool(new PoolSettings(stageStarPrefab.name, stageStarPrefab, 1, true));

            characterPanels = new CharacterPanelUI[charactersDatabase.Characters.Length];
            for (int i = 0; i < characterPanels.Length; i++)
            {
                GameObject tempCharacter = Instantiate(panelUIPrefab);
                tempCharacter.transform.SetParent(panelsContainer);
                tempCharacter.transform.ResetLocal();

                characterPanels[i] = tempCharacter.GetComponent<CharacterPanelUI>();
                characterPanels[i].Initialise(charactersDatabase.Characters[i], this);
            }

            currencies = CurrenciesController.Currencies;

            if (UIController.IsTablet)
            {
                var scrollSize = backgroundPanelRectTransform.sizeDelta;
                scrollSize.y += 60;
                backgroundPanelRectTransform.sizeDelta = scrollSize;
            }
        }

        private void OnCurrencyAmountChanged(Currency currency, int difference)
        {
            for (int i = 0; i < characterPanels.Length; i++)
            {
                characterPanels[i].OnMoneyAmountChanged();
            }
        }

        public override void PlayShowAnimation()
        {
            // Subscribe events
            for (int i = 0; i < currencies.Length; i++)
            {
                currencies[i].OnCurrencyChanged += OnCurrencyAmountChanged;
            }

            backgroundPanelRectTransform.anchoredPosition = new Vector2(0, -1500);
            backgroundPanelRectTransform.DOAnchoredPosition(Vector2.zero, 0.3f).SetCustomEasing(bounceEasingFunction);

            int selectedIndex = Mathf.Clamp(CharactersController.GetCharacterIndex(CharactersController.SelectedCharacter.Type), 0, int.MaxValue);

            float scrollOffsetX = -(characterPanels[selectedIndex].RectTransform.anchoredPosition.x - SCROLL_ELEMENT_WIDTH - SCROLL_SIDE_OFFSET);
            scrollView.content.anchoredPosition = new Vector2(scrollOffsetX, 0);
            scrollView.StopMovement();

            ResetAnimations();

            for (int i = 0; i < characterPanels.Length; i++)
            {
                RectTransform panelTransform = characterPanels[i].RectTransform;

                panelTransform.localScale = Vector2.zero;

                if (i == selectedIndex)
                {
                    panelTransform.DOScale(Vector3.one, 0.3f, 0.2f).SetCurveEasing(selectedPanelScaleAnimationCurve);
                }
                else
                {
                    panelTransform.DOScale(Vector3.one, 0.3f, 0.3f).SetCurveEasing(panelScaleAnimationCurve);
                }

                characterPanels[i].OnPanelOpened();
            }

            UIGeneralPowerIndicator.Show();

            Tween.DelayedCall(0.5f, () => UIController.OnPageOpened(this));

            StartAnimations();

            UIMainMenu.DotsBackground.gameObject.SetActive(true);
        }

        private void ResetAnimations()
        {
            if (isAnimationPlaying)
            {
                StopCoroutine(animationCoroutine);

                isAnimationPlaying = false;
                animationCoroutine = null;
            }

            characterDynamicAnimations = new List<CharacterDynamicAnimation>();
        }

        private void StartAnimations()
        {
            if (isAnimationPlaying)
                return;

            if (!characterDynamicAnimations.IsNullOrEmpty())
            {
                isControlBlocked = true;
                scrollView.enabled = false;

                isAnimationPlaying = true;

                animationCoroutine = StartCoroutine(DynamicAnimationCoroutine());
            }
        }

        private IEnumerator ScrollCoroutine(CharacterPanelUI characterPanelUI)
        {
            float scrollOffsetX = -(characterPanelUI.RectTransform.anchoredPosition.x - SCROLL_ELEMENT_WIDTH - SCROLL_SIDE_OFFSET);

            float positionDiff = Mathf.Abs(scrollView.content.anchoredPosition.x - scrollOffsetX);

            if (positionDiff > 80)
            {
                Ease.IEasingFunction easeFunctionCubicIn = Ease.GetFunction(Ease.Type.CubicOut);

                Vector2 currentPosition = scrollView.content.anchoredPosition;
                Vector2 targetPosition = new Vector2(scrollOffsetX, 0);

                float speed = positionDiff / 2500;

                for (float s = 0; s < 1.0f; s += Time.deltaTime / speed)
                {
                    scrollView.content.anchoredPosition = Vector2.Lerp(currentPosition, targetPosition, easeFunctionCubicIn.Interpolate(s));

                    yield return null;
                }
            }
        }

        private IEnumerator DynamicAnimationCoroutine()
        {
            int currentAnimationIndex = 0;
            CharacterDynamicAnimation tempAnimation;
            WaitForSeconds delayWait = new WaitForSeconds(0.4f);

            yield return delayWait;

            while (currentAnimationIndex < characterDynamicAnimations.Count)
            {
                tempAnimation = characterDynamicAnimations[currentAnimationIndex];

                delayWait = new WaitForSeconds(tempAnimation.Delay);

                yield return StartCoroutine(ScrollCoroutine(tempAnimation.CharacterPanel));

                tempAnimation.OnAnimationStarted?.Invoke();

                yield return delayWait;

                currentAnimationIndex++;
            }

            yield return null;

            isAnimationPlaying = false;
            isControlBlocked = false;
            scrollView.enabled = true;
        }

        public void AddAnimations(List<CharacterDynamicAnimation> characterDynamicAnimation, bool isPrioritize = false)
        {
            if (!isPrioritize)
            {
                characterDynamicAnimations.AddRange(characterDynamicAnimation);
            }
            else
            {
                characterDynamicAnimations.InsertRange(0, characterDynamicAnimation);
            }
        }

        public void ScrollToPanel(CharacterPanelUI characterPanelUI)
        {
            float scrollOffsetX = -(characterPanelUI.RectTransform.anchoredPosition.x - SCROLL_ELEMENT_WIDTH - SCROLL_SIDE_OFFSET);

            float positionDiff = Mathf.Abs(scrollView.content.anchoredPosition.x - scrollOffsetX);

            if (positionDiff > 80)
            {
                scrollView.StopMovement();
                scrollView.enabled = false;

                scrollView.content.DOAnchoredPosition(new Vector2(scrollOffsetX, 0), positionDiff / 2500).SetEasing(Ease.Type.CubicOut).OnComplete(delegate
                {
                    scrollView.enabled = true;
                });
            }
        }

        public override void PlayHideAnimation()
        {
            UIMainMenu.DontFadeRevealNextTime = true;

            // Unsubscribe events
            for (int i = 0; i < currencies.Length; i++)
            {
                currencies[i].OnCurrencyChanged -= OnCurrencyAmountChanged;
            }

            UIGeneralPowerIndicator.Hide();

            backgroundPanelRectTransform.DOAnchoredPosition(new Vector2(0, -1500), 0.3f).SetEasing(Ease.Type.CubicIn).OnComplete(delegate
            {
                UIController.OnPageClosed(this);
            });
        }

        public GameObject GetStageStarObject()
        {
            return stageStarPool.GetPooledObject();
        }

        public CharacterPanelUI GetPanel(CharacterType characterType)
        {
            for (int i = 0; i < characterPanels.Length; i++)
            {
                if (characterPanels[i].Character.Type == characterType)
                    return characterPanels[i];
            }

            return null;
        }

        public bool IsAnyActionAvailable()
        {
            for (int i = 0; i < characterPanels.Length; i++)
            {
                if (characterPanels[i].IsNewCharacterOpened())
                    return true;

                if (characterPanels[i].IsNextUpgradeCanBePurchased())
                    return true;
            }

            return false;
        }

        #region Buttons
        public void BackButton()
        {
            UIController.HidePage<UICharactersPanel>(() =>
            {
                UIController.ShowPage<UIMainMenu>();
            });

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }
        #endregion
    }
}