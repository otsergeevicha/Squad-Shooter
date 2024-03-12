using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;

namespace Watermelon.SquadShooter
{


    public class CharacterPanelUI : MonoBehaviour
    {
        private const string LOCKED_NAME = "???";
        private const string UPGRADE_TEXT = "UPGRADE";
        private const string EVOLVE_TEXT = "EVOLVE";

        [SerializeField] Image previewImage;
        [SerializeField] TextMeshProUGUI titleText;

        [Header("Selection")]
        [SerializeField] Image selectionImage;
        [SerializeField] Transform backgroundTransform;

        [Header("Power")]
        [SerializeField] GameObject powerObject;
        [SerializeField] TextMeshProUGUI powerText;

        [Header("Upgrades")]
        [SerializeField] GameObject upgradesStateObject;

        [Space]
        [SerializeField] Color upgradeStateActiveColor = Color.white;
        [SerializeField] Color upgradeStateUnactiveColor = Color.white;
        [SerializeField] Image[] upgradesStatesImages;

        [Space]
        [SerializeField] Button upgradesBuyButton;
        [SerializeField] Image upgradesBuyButtonImage;
        [SerializeField] TextMeshProUGUI upgradesBuyButtonText;
        [SerializeField] Sprite upgradesBuyButtonActiveSprite;
        [SerializeField] Sprite upgradesBuyButtonDisableSprite;
        [SerializeField] TextMeshProUGUI upgradesText;

        [Space]
        [SerializeField] GameObject upgradesMaxObject;

        [Header("Locked State")]
        [SerializeField] GameObject lockedStateObject;
        [SerializeField] TextMeshProUGUI lockedStateText;
        [SerializeField] Color lockedPreviewColor = Color.white;

        private Character character;
        public Character Character => character;

        private bool storedIsLocked;

        private RectTransform panelRectTransform;
        public RectTransform RectTransform => panelRectTransform;

        public Transform UpgradeButtonTransform => upgradesBuyButton.transform;

        private UICharactersPanel charactersPanel;

        private bool isUpgradeAnimationPlaying;

        private static CharacterPanelUI selectedCharacterPanelUI;

        public void Initialise(Character character, UICharactersPanel charactersPanel)
        {
            this.character = character;
            this.charactersPanel = charactersPanel;

            panelRectTransform = (RectTransform)transform;

            previewImage.sprite = character.GetCurrentStage().PreviewSprite;

            for (int i = 0; i < upgradesStatesImages.Length; i++)
            {
                if (character.Upgrades.IsInRange(i + 1))
                {
                    if (character.Upgrades[i + 1].ChangeStage)
                    {
                        GameObject stageStarObject = charactersPanel.GetStageStarObject();
                        stageStarObject.transform.SetParent(upgradesStatesImages[i].rectTransform);
                        stageStarObject.transform.ResetLocal();

                        RectTransform stageStarRectTransform = (RectTransform)stageStarObject.transform;
                        stageStarRectTransform.sizeDelta = new Vector2(19.4f, 19.4f);
                        stageStarRectTransform.anchoredPosition = Vector2.zero;

                        stageStarObject.SetActive(true);
                    }
                }
            }

            if (character.IsUnlocked())
            {
                titleText.text = character.Name.ToUpper();

                storedIsLocked = false;

                if (CharactersController.SelectedCharacter.Type == character.Type)
                    SelectCharacter();

                lockedStateObject.SetActive(false);
                upgradesStateObject.SetActive(true);
                powerObject.SetActive(true);

                RedrawUpgradeElements();
                RedrawPower();
            }
            else
            {
                titleText.text = LOCKED_NAME;

                storedIsLocked = true;

                powerObject.SetActive(false);

                previewImage.sprite = character.LockedSprite;
                previewImage.color = lockedPreviewColor;

                SetRequiredLevel(character.RequiredLevel);
            }
        }

        private void PlayOpenAnimation()
        {
            lockedStateObject.SetActive(false);

            powerObject.SetActive(true);
            upgradesStateObject.SetActive(true);

            titleText.text = character.Name.ToUpper();

            previewImage.sprite = character.Stages[0].PreviewSprite;
            previewImage.DOColor(Color.white, 0.6f);

            RedrawUpgradeElements();

            RedrawPower();
        }

        private void PlayUpgradeAnimation(int stage)
        {
            CharacterStageData tempStage = character.Stages[stage];

            previewImage.sprite = tempStage.PreviewSprite;
            previewImage.rectTransform.localScale = Vector2.one * 1.3f;

            previewImage.rectTransform.DOScale(1.0f, 0.2f, 0.03f).SetEasing(Ease.Type.SineIn);
        }

        public void OnPanelOpened()
        {
            List<CharacterDynamicAnimation> dynamicAnimations = new List<CharacterDynamicAnimation>();

            bool isSelected = character.IsSelected();

            // Character was locked on start
            if (storedIsLocked)
            {
                // Now character is unlocked
                if (character.IsUnlocked())
                {
                    storedIsLocked = false;

                    // Play unlock animation
                    CharacterDynamicAnimation unlockAnimation = new CharacterDynamicAnimation(this, 0.5f, onAnimationStarted: PlayOpenAnimation);

                    dynamicAnimations.Add(unlockAnimation);
                }
            }

            if (!dynamicAnimations.IsNullOrEmpty())
            {
                charactersPanel.AddAnimations(dynamicAnimations, isSelected);
            }

            RedrawUpgradeElements();
            RedrawPower();
        }

        public void SetRequiredLevel(int level)
        {
            lockedStateObject.SetActive(true);
            lockedStateText.text = level.ToString();
        }

        public void SelectCharacter()
        {
            if (selectedCharacterPanelUI != null)
                selectedCharacterPanelUI.UnselectCharacter();

            selectionImage.gameObject.SetActive(true);

            selectedCharacterPanelUI = this;

            UIGeneralPowerIndicator.UpdateText();
        }

        public void UnselectCharacter()
        {
            selectionImage.gameObject.SetActive(false);
        }

        private void PlayUpgradeAnimation()
        {
            int upgradeStateIndex = character.GetCurrentUpgradeIndex() - 1;
            upgradesStatesImages[upgradeStateIndex].DOColor(upgradeStateActiveColor, 0.3f).OnComplete(delegate
            {
                isUpgradeAnimationPlaying = false;

                RedrawUpgradeButton();
            });

            if (!character.IsMaxUpgrade())
            {
                upgradesMaxObject.SetActive(false);
                upgradesBuyButton.gameObject.SetActive(true);
            }
            else
            {
                upgradesMaxObject.SetActive(true);
                upgradesBuyButton.gameObject.SetActive(false);
            }
        }

        public void OnMoneyAmountChanged()
        {
            if (isUpgradeAnimationPlaying)
                return;

            RedrawUpgradeButton();
        }

        private void RedrawPower()
        {
            CharacterUpgrade currentUpgrade = character.GetCurrentUpgrade();

            // Redraw character power
            powerText.text = currentUpgrade.Stats.Power.ToString();
        }

        private void RedrawUpgradeButton()
        {
            if (!character.IsMaxUpgrade())
            {
                CharacterUpgrade upgradeState = character.Upgrades[character.GetCurrentUpgradeIndex() + 1];

                int price = upgradeState.Price;
                if (CurrenciesController.HasAmount(CurrencyType.Coin, price))
                {
                    upgradesBuyButtonImage.sprite = upgradesBuyButtonActiveSprite;
                }
                else
                {
                    upgradesBuyButtonImage.sprite = upgradesBuyButtonDisableSprite;
                }

                upgradesBuyButtonText.text = CurrenciesHelper.Format(price);

                if (upgradeState.ChangeStage)
                {
                    upgradesText.text = EVOLVE_TEXT;
                }
                else
                {
                    upgradesText.text = UPGRADE_TEXT;
                }
            }
        }

        public bool IsNewCharacterOpened()
        {
            return storedIsLocked && character.IsUnlocked();
        }

        public bool IsNextUpgradeCanBePurchased()
        {
            if (character.IsUnlocked())
            {
                if (!character.IsMaxUpgrade())
                {
                    CharacterUpgrade upgradeState = character.Upgrades[character.GetCurrentUpgradeIndex() + 1];

                    if (CurrenciesController.HasAmount(CurrencyType.Coin, upgradeState.Price))
                        return true;
                }
            }

            return false;
        }

        private void RedrawUpgradeElements()
        {
            if (!character.IsMaxUpgrade())
            {
                int upgradeStateIndex = character.GetCurrentUpgradeIndex();
                for (int i = 0; i < upgradeStateIndex; i++)
                {
                    upgradesStatesImages[i].color = upgradeStateActiveColor;
                }

                upgradesMaxObject.SetActive(false);
                upgradesBuyButton.gameObject.SetActive(true);

                RedrawUpgradeButton();
            }
            else
            {
                for (int i = 0; i < upgradesStatesImages.Length; i++)
                {
                    upgradesStatesImages[i].color = upgradeStateActiveColor;
                }

                upgradesMaxObject.SetActive(true);
                upgradesBuyButton.gameObject.SetActive(false);
            }
        }

        public void OnUpgradeButtonClicked()
        {
            if (UICharactersPanel.IsControlBlocked)
                return;

            if (!character.IsMaxUpgrade())
            {
                OnSelectButtonClicked();

                int upgradeStateIndex = character.GetCurrentUpgradeIndex() + 1;

                int price = character.Upgrades[upgradeStateIndex].Price;
                if (CurrenciesController.HasAmount(CurrencyType.Coin, price))
                {
                    isUpgradeAnimationPlaying = true;

                    CurrenciesController.Substract(CurrencyType.Coin, price);

                    character.UpgradeCharacter();

                    if (CharactersController.SelectedCharacter.Type == character.Type)
                    {
                        CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();

                        CharacterUpgrade currentUpgrade = character.GetCurrentUpgrade();
                        if (currentUpgrade.ChangeStage)
                        {
                            PlayUpgradeAnimation(currentUpgrade.StageIndex);

                            characterBehaviour.SetGraphics(character.Stages[currentUpgrade.StageIndex].Prefab, true, true);
                        }
                        else
                        {
                            BaseCharacterGraphics characterGraphics = characterBehaviour.Graphics;
                            characterGraphics.PlayUpgradeParticle();
                            characterGraphics.PlayBounceAnimation();
                        }

                        // Update character stats
                        characterBehaviour.SetStats(currentUpgrade.Stats);
                    }

                    PlayUpgradeAnimation();

                    RedrawUpgradeButton();

                    RedrawPower();

                    UIGeneralPowerIndicator.UpdateText(true);
                }
                else
                {
                    // Play disable button click sound
                }

                AudioController.PlaySound(AudioController.Sounds.buttonSound);
            }
        }

        public void OnSelectButtonClicked()
        {
            if (UICharactersPanel.IsControlBlocked)
                return;

            if (character.IsSelected())
                return;

            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            // Check if character is unlocked
            if (!character.IsUnlocked())
                return;

            CharactersController.SelectCharacter(character.Type);

            SelectCharacter();
        }
    }
}