using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

namespace Watermelon.SquadShooter
{
    public class UICharacterSuggestion : UIPage
    {
        [SerializeField] RectTransform panelRectTransform;
        [SerializeField] GameObject nextCharacterText;
        [SerializeField] GameObject characterUnlockedText;
        [SerializeField] Image characterImage;
        [SerializeField] GameObject fillbarObject;
        [SerializeField] Image characterFillImage;
        [SerializeField] GameObject lightBeamImage;
        [SerializeField] SlicedFilledImage fillbarImage;
        [SerializeField] GameObject unlockedTextObject;
        [SerializeField] GameObject continueText;
        [SerializeField] TextMeshProUGUI persentageText;

        private float lastProgression;
        private float currentProgression;
        private Character characterData;

        public override void Initialise()
        {

        }

        public void SetData(float lastProgression, float currentProgression, Character characterData)
        {
            this.lastProgression = lastProgression;
            this.currentProgression = currentProgression;
            this.characterData = characterData;
        }

        public override void PlayShowAnimation()
        {
            nextCharacterText.SetActive(true);
            characterUnlockedText.SetActive(false);
            lightBeamImage.SetActive(false);
            unlockedTextObject.SetActive(false);
            continueText.SetActive(false);
            fillbarObject.SetActive(true);

            panelRectTransform.sizeDelta = new Vector2(0, 335f);

            characterImage.sprite = characterData.Stages[characterData.Stages.Length - 1].PreviewSprite;
            characterFillImage.sprite = characterData.Stages[characterData.Stages.Length - 1].LockedSprite;

            characterFillImage.fillAmount = 1f - lastProgression;
            fillbarImage.fillAmount = lastProgression;

            persentageText.text = ((int)(currentProgression * 100)).ToString() + "%";

            panelRectTransform.DOSize(new Vector2(0, 915), 0.4f).SetEasing(Ease.Type.BackOut).OnComplete(() =>
            {
                fillbarImage.DOFillAmount(currentProgression, 0.6f).SetEasing(Ease.Type.CubicOut);
                characterFillImage.DOFillAmount(1f - currentProgression, 0.6f).SetEasing(Ease.Type.CubicOut).OnComplete(() =>
                {
                    continueText.SetActive(true);

                    if (currentProgression >= 1f)
                    {
                        nextCharacterText.SetActive(false);
                        characterUnlockedText.SetActive(true);
                        lightBeamImage.SetActive(true);
                        unlockedTextObject.SetActive(true);
                        continueText.SetActive(true);
                        fillbarObject.SetActive(false);

                        lightBeamImage.transform.localScale = Vector3.zero;
                        lightBeamImage.transform.DOScale(1f, 0.3f).SetEasing(Ease.Type.BackOut);
                    }

                    UIController.OnPageOpened(this);
                });
            });
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);

            GameController.OnCharacterSugessionClosed();
        }

        #region Buttons
        public void ContinueButton()
        {
            UIController.HidePage<UICharacterSuggestion>();

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }
        #endregion
    }
}