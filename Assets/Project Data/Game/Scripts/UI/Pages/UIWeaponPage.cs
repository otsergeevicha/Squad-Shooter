using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Watermelon;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class UIWeaponPage : UIPage
    {
        private const float SCROLL_SIDE_OFFSET = 50;
        private const float SCROLL_ELEMENT_WIDTH = 415f;

        [SerializeField] Transform weaponHolder;
        [SerializeField] GameObject weaponPanelPrefab;

        [Space]
        [SerializeField] RectTransform backgroundPanelRectTransform;
        [SerializeField] RectTransform closeButtonRectTransform;
        [SerializeField] ScrollRect weaponsScrollView;

        [Space]
        [SerializeField] AnimationCurve movementAnimationCurve;
        [SerializeField] AnimationCurve panelScaleAnimationCurve;
        [SerializeField] AnimationCurve selectedPanelScaleAnimationCurve;

        private PoolGeneric<WeaponPanelUI> weaponPanelPool;
        private WeaponsController weaponController;

        private List<WeaponPanelUI> weaponPanels = new List<WeaponPanelUI>();

        private Currency[] currencies;

        private void Awake()
        {
            weaponPanelPool = new PoolGeneric<WeaponPanelUI>(new PoolSettings(weaponPanelPrefab.name, weaponPanelPrefab, 3, true, weaponHolder));
        }

        public void Initiaise(WeaponsController weaponController)
        {
            this.weaponController = weaponController;

            InitiasePanels();

            WeaponsController.OnWeaponUnlocked += OnWeaponUnlockedOrUpgraded;
            WeaponsController.OnWeaponUpgraded += UpdateWeaponPanel;

            currencies = CurrenciesController.Currencies;

            if (UIController.IsTablet)
            {
                var scrollSize = backgroundPanelRectTransform.sizeDelta;
                scrollSize.y += 60;
                backgroundPanelRectTransform.sizeDelta = scrollSize;
            }
        }

        private void InitiasePanels()
        {
            for (int i = 0; i < WeaponsController.Database.Weapons.Length; i++)
            {
                WeaponPanelUI weaponPanel = weaponPanelPool.GetPooledComponent();

                BaseUpgrade upgrade = UpgradesController.GetUpgrade<BaseUpgrade>(WeaponsController.Database.Weapons[i].UpgradeType);

                weaponPanel.Init(weaponController, upgrade as BaseWeaponUpgrade, WeaponsController.Database.Weapons[i], i);
                weaponPanels.Add(weaponPanel);
            }
        }

        private void OnCurrencyAmountChanged(Currency currency, int difference)
        {
            for (int i = 0; i < weaponPanels.Count; i++)
            {
                weaponPanels[i].OnMoneyAmountChanged();
            }
        }

        private void OnWeaponUnlockedOrUpgraded(WeaponData weapon)
        {
            UpdateWeaponPanel();
        }

        public void UpdateWeaponPanel()
        {
            for (int i = 0; i < weaponPanels.Count; i++)
            {
                weaponPanels[i].UpdateUI();
            }
        }

        public bool IsAnyActionAvailable()
        {
            for (int i = 0; i < weaponPanels.Count; i++)
            {
                if (weaponPanels[i].IsNextUpgradeCanBePurchased())
                    return true;
            }

            return false;
        }

        #region UI Page

        public override void Initialise()
        {

        }

        public override void PlayShowAnimation()
        {
            UpdateWeaponPanel();
            OverlayUI.ShowOverlay();

            for (int i = 0; i < currencies.Length; i++)
            {
                currencies[i].OnCurrencyChanged += OnCurrencyAmountChanged;
            }

            backgroundPanelRectTransform.anchoredPosition = new Vector2(0, -1500);
            backgroundPanelRectTransform.DOAnchoredPosition(Vector2.zero, 0.3f).SetCurveEasing(movementAnimationCurve);

            int selectedWeaponIndex = Mathf.Clamp(WeaponsController.SelectedWeaponIndex, 0, int.MaxValue);

            float scrollOffsetX = -(weaponPanels[selectedWeaponIndex].RectTransform.anchoredPosition.x - SCROLL_ELEMENT_WIDTH - SCROLL_SIDE_OFFSET);
            weaponsScrollView.content.anchoredPosition = new Vector2(scrollOffsetX, 0);
            weaponsScrollView.StopMovement();

            for (int i = 0; i < weaponPanels.Count; i++)
            {
                RectTransform panelTransform = weaponPanels[i].RectTransform;

                panelTransform.localScale = Vector2.zero;

                if (i == selectedWeaponIndex)
                {
                    panelTransform.DOScale(Vector3.one, 0.3f, 0.2f).SetCurveEasing(selectedPanelScaleAnimationCurve);
                }
                else
                {
                    panelTransform.DOScale(Vector3.one, 0.3f, 0.3f).SetCurveEasing(panelScaleAnimationCurve);
                }

                weaponPanels[i].OnPanelOpened();
            }

            UIGeneralPowerIndicator.Show();

            Tween.DelayedCall(0.9f, () => UIController.OnPageOpened(this));
        }

        public override void PlayHideAnimation()
        {
            UIGeneralPowerIndicator.Hide();

            UIMainMenu.DontFadeRevealNextTime = true;

            UIController.OnPageClosed(this);

            for (int i = 0; i < currencies.Length; i++)
            {
                currencies[i].OnCurrencyChanged -= OnCurrencyAmountChanged;
            }
        }

        public WeaponPanelUI GetWeaponPanel(WeaponType weaponType)
        {
            for (int i = 0; i < weaponPanels.Count; i++)
            {
                if (weaponPanels[i].Data.Type == weaponType)
                    return weaponPanels[i];
            }

            return null;
        }

        #endregion

        #region Buttons
        public void BackButton()
        {
            UIController.HidePage<UIWeaponPage>(() =>
            {
                UIController.ShowPage<UIMainMenu>();
            });

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
        }
        #endregion
    }
}