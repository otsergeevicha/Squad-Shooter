using Watermelon.LevelSystem;

namespace Watermelon.SquadShooter
{
    public class CharacterUpgradeTutorial : ITutorial
    {
        private const CharacterType FIRST_CHARACTER_TYPE = CharacterType.Character_01;

        public TutorialID TutorialID => TutorialID.CharacterUpgrade;

        public bool IsActive => saveData.isActive;
        public bool IsFinished => saveData.isFinished;
        public int Progress => saveData.progress;

        private TutorialBaseSave saveData;

        private Character firstCharacter;

        private UIMainMenu mainMenuUI;
        private UICharactersPanel characterPanelUI;
        private CharacterTab characterTab;

        private bool isActive;

        private bool isInitialised;
        public bool IsInitialised => isInitialised;

        public CharacterUpgradeTutorial()
        {
            TutorialController.RegisterTutorial(this);
        }

        public void Initialise()
        {
            if (isInitialised)
                return;

            isInitialised = true;

            // Load save file
            saveData = SaveController.GetSaveObject<TutorialBaseSave>(string.Format(ITutorial.SAVE_IDENTIFIER, TutorialID.ToString()));

            firstCharacter = CharactersController.GetCharacter(FIRST_CHARACTER_TYPE);

            mainMenuUI = UIController.GetPage<UIMainMenu>();
            characterPanelUI = UIController.GetPage<UICharactersPanel>();

            characterTab = mainMenuUI.CharacterTab;
        }

        public void StartTutorial()
        {
            if (isActive)
                return;

            isActive = true;

            UIController.OnPageOpenedEvent += OnMainMenuPageOpened;
        }

        private void OnMainMenuPageOpened(UIPage page, System.Type pageType)
        {
            if(pageType == typeof(UIMainMenu))
            {
                if (ActiveRoom.CurrentLevelIndex >= 1)
                {
                    // Player has enough money to upgrade first character
                    if (CurrenciesController.HasAmount(firstCharacter.Upgrades[1].CurrencyType, firstCharacter.Upgrades[1].Price))
                    {
                        UIController.OnPageOpenedEvent -= OnMainMenuPageOpened;

                        characterTab.Button.onClick.AddListener(OnCharacterTabOpened);

                        TutorialCanvasController.ActivateTutorialCanvas(mainMenuUI.CharacterTab.RectTransform, false, true);
                        TutorialCanvasController.ActivatePointer(mainMenuUI.CharacterTab.RectTransform.position, TutorialCanvasController.POINTER_TOPDOWN);
                    }
                }
            }
        }

        private void OnCharacterTabOpened()
        {
            TutorialCanvasController.ResetTutorialCanvas();

            characterTab.Button.onClick.RemoveListener(OnCharacterTabOpened);

            characterPanelUI.GraphicRaycaster.enabled = false;

            UIController.OnPageOpenedEvent += OnCharacterPageOpened;
        }

        private void OnCharacterPageOpened(UIPage page, System.Type pageType)
        {
            UIController.OnPageOpenedEvent -= OnCharacterPageOpened;

            CharacterPanelUI characterPanel = characterPanelUI.GetPanel(FIRST_CHARACTER_TYPE);
            if (characterPanel != null)
            {
                TutorialCanvasController.ActivateTutorialCanvas(characterPanel.RectTransform, true, true);
                TutorialCanvasController.ActivatePointer(characterPanel.UpgradeButtonTransform.position, TutorialCanvasController.POINTER_TOPDOWN);

                CharactersController.OnCharacterUpgradedEvent += OnCharacterUpgraded;
            }

            characterPanelUI.GraphicRaycaster.enabled = true;
        }

        private void OnCharacterUpgraded(CharacterType characterType, Character character)
        {
            CharactersController.OnCharacterUpgradedEvent -= OnCharacterUpgraded;

            TutorialCanvasController.ResetTutorialCanvas();

            FinishTutorial();
        }

        public void FinishTutorial()
        {
            saveData.isFinished = true;
        }

        public void Unload()
        {

        }
    }
}