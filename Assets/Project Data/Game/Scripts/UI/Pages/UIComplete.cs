using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Watermelon.SquadShooter;

namespace Watermelon
{
    public class UIComplete : UIPage
    {
        private const string LEVEL_TEXT = "LEVEL {0}-{1}";
        private const string PLUS_TEXT = "+{0}";

        [SerializeField] DotsBackground dotsBackground;
        [SerializeField] RectTransform panelRectTransform;

        [SerializeField] CanvasGroup panelContentCanvasGroup;
        [SerializeField] TextMeshProUGUI levelText;

        [Space]
        [SerializeField] GameObject dropCardPrefab;
        [SerializeField] Transform cardsContainerTransform;

        [Space]
        [SerializeField] TextMeshProUGUI experienceGainedText;
        [SerializeField] TextMeshProUGUI moneyGainedText;

        [Space]
        [SerializeField] BlackFadeBehavior blackFade;

        private int currentWorld;
        private int currentLevel;
        private int collectedMoney;
        private int collectedExperience;
        private List<WeaponType> collectedCards;

        private Pool cardsUIPool;

        public override void Initialise()
        {
            cardsUIPool = new Pool(new PoolSettings(dropCardPrefab.name, dropCardPrefab, 1, true, cardsContainerTransform));
        }

        public void SetData(int currentWorld, int currentLevel, int collectedMoney, int collectedExperience, List<WeaponType> collectedCards)
        {
            this.currentWorld = currentWorld;
            this.currentLevel = currentLevel;
            this.collectedMoney = collectedMoney;
            this.collectedExperience = collectedExperience;
            this.collectedCards = collectedCards;
        }

        #region Show/Hide
        public override void PlayShowAnimation()
        {
            float showTime = 0.7f;

            dotsBackground.ApplyParams();

            cardsUIPool.ReturnToPoolEverything();

            // RESET
            panelRectTransform.sizeDelta = new Vector2(0, 335f);
            dotsBackground.BackgroundImage.color = Color.white.SetAlpha(0.0f);
            panelContentCanvasGroup.alpha = 0;

            levelText.text = string.Format(LEVEL_TEXT, currentWorld, currentLevel);

            dotsBackground.BackgroundImage.DOColor(Color.white, 0.3f);
            panelContentCanvasGroup.DOFade(1.0f, 0.3f, 0.1f);

            moneyGainedText.text = "0";
            Tween.DoFloat(0, collectedMoney, 0.4f, (result) =>
            {
                moneyGainedText.text = string.Format(PLUS_TEXT, result.ToString("00"));
            }, 0.2f);

            experienceGainedText.text = "0";
            Tween.DoFloat(0, collectedExperience, 0.4f, (result) =>
            {
                experienceGainedText.text = string.Format(PLUS_TEXT, result.ToString("00"));
            }, 0.3f);

            bool cardsDropped = !collectedCards.IsNullOrEmpty();
            if(cardsDropped)
            {
                List<WeaponType> uniqueCards = new List<WeaponType>();
                for(int i = 0; i < collectedCards.Count; i++)
                {
                    if(uniqueCards.FindIndex(x => x == collectedCards[i]) == -1)
                    {
                        uniqueCards.Add(collectedCards[i]);
                    }
                }

                for (int i = 0; i < uniqueCards.Count; i++)
                {
                    GameObject cardUIObject = cardsUIPool.GetPooledObject();
                    cardUIObject.SetActive(true);

                    DroppedCardPanel droppedCardPanel = cardUIObject.GetComponent<DroppedCardPanel>();
                    droppedCardPanel.Initialise(uniqueCards[i]);

                    CanvasGroup droppedCardCanvasGroup = droppedCardPanel.CanvasGroup;
                    droppedCardCanvasGroup.alpha = 0.0f;
                    droppedCardCanvasGroup.DOFade(1.0f, 0.5f, 0.1f * i + 0.45f).OnComplete(delegate
                    {
                        droppedCardPanel.OnDisplayed();
                    });
                }

                panelRectTransform.DOSize(new Vector2(0, 815), 0.4f).SetEasing(Ease.Type.BackOut);

                showTime = 1.1f;
            }

            Tween.DelayedCall(showTime, () => UIController.OnPageOpened(this));
        }

        public override void PlayHideAnimation()
        {
            if (!isPageDisplayed)
                return;

            blackFade.Hide(onComplete: () => UIController.OnPageClosed(this));
        }

        #endregion

        #region Experience
        public void UpdateExperienceLabel(int experienceGained)
        {
            experienceGainedText.text = experienceGained.ToString();
        }

        #endregion

        #region Buttons
        public void ContinueButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            GameController.OnLevelCompleteClosed();
        }
        #endregion
    }
}
