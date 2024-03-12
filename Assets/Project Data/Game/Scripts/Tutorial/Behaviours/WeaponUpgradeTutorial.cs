using UnityEngine;
using Watermelon.LevelSystem;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class WeaponUpgradeTutorial : ITutorial
    {
        private const WeaponType FIRST_WEAPON_TYPE = WeaponType.Minigun;

        public TutorialID TutorialID => TutorialID.WeaponUpgrade;

        public bool IsActive => saveData.isActive;
        public bool IsFinished => saveData.isFinished;
        public int Progress => saveData.progress;

        private TutorialBaseSave saveData;

        private WeaponData weaponData;
        private BaseWeaponUpgrade weaponUpgrade;

        private UIMainMenu mainMenuUI;
        private UIWeaponPage weaponPageUI;

        private WeaponTab weaponTab;
        private CharacterTab characterTab;

        private bool isActive;

        private bool isInitialised;
        public bool IsInitialised => isInitialised;

        public WeaponUpgradeTutorial()
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

            weaponData = WeaponsController.GetWeaponData(FIRST_WEAPON_TYPE);
            weaponUpgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(weaponData.UpgradeType);

            mainMenuUI = UIController.GetPage<UIMainMenu>();
            weaponPageUI = UIController.GetPage<UIWeaponPage>();

            weaponTab = mainMenuUI.WeaponTab;
            characterTab = mainMenuUI.CharacterTab;
        }

        public void StartTutorial()
        {
            if (isActive)
                return;

            isActive = true;

            weaponTab.Disable();

            UIController.OnPageOpenedEvent += OnMainMenuPageOpened;
        }

        private void OnMainMenuPageOpened(UIPage page, System.Type pageType)
        {
            if (pageType == typeof(UIMainMenu))
            {
                if (ActiveRoom.CurrentLevelIndex >= 2)
                {
                    // Player has enough money to upgrade first weapon
                    if (CurrenciesController.HasAmount(weaponUpgrade.Upgrades[1].CurrencyType, weaponUpgrade.Upgrades[1].Price))
                    {
                        UIController.OnPageOpenedEvent -= OnMainMenuPageOpened;

                        characterTab.Disable();

                        weaponTab.Activate();
                        weaponTab.Button.onClick.AddListener(OnWeaponTabOpened);

                        TutorialCanvasController.ActivateTutorialCanvas(weaponTab.RectTransform, false, true);
                        TutorialCanvasController.ActivatePointer(weaponTab.RectTransform.position + new Vector3(0, 0.1f, 0), TutorialCanvasController.POINTER_TOPDOWN);
                    }
                }
            }
        }

        private void OnWeaponTabOpened()
        {
            TutorialCanvasController.ResetTutorialCanvas();

            weaponTab.Button.onClick.RemoveListener(OnWeaponTabOpened);

            UIController.OnPageOpenedEvent += OnWeaponPageOpened;

            weaponPageUI.GraphicRaycaster.enabled = false;
        }

        private void OnWeaponPageOpened(UIPage page, System.Type pageType)
        {
            UIController.OnPageOpenedEvent -= OnWeaponPageOpened;

            WeaponPanelUI weaponPanel = weaponPageUI.GetWeaponPanel(FIRST_WEAPON_TYPE);
            if (weaponPanel != null)
            {
                TutorialCanvasController.ActivateTutorialCanvas(weaponPanel.RectTransform, true, true);
                TutorialCanvasController.ActivatePointer(weaponPanel.UpgradeButtonTransform.position, TutorialCanvasController.POINTER_TOPDOWN);

                WeaponsController.OnWeaponUpgraded += OnWeaponUpgraded;

                if(WeaponsController.IsTutorialWeaponUpgraded())
                {
                    OnWeaponUpgraded();
                }
            }

            weaponPageUI.GraphicRaycaster.enabled = true;
        }

        private void OnWeaponUpgraded()
        {
            WeaponsController.OnWeaponUpgraded -= OnWeaponUpgraded;

            TutorialCanvasController.ResetTutorialCanvas();

            characterTab.Activate();

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