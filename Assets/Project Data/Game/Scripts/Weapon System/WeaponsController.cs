using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Watermelon;
using Watermelon.LevelSystem;
using Watermelon.Upgrades;

namespace Watermelon.SquadShooter
{
    public class WeaponsController : MonoBehaviour
    {
        [SerializeField] WeaponDatabase database;
        public static WeaponDatabase Database => instance.database;

        [Header("Drop")]
        [SerializeField] GameObject cardPrefab;

        private static WeaponsController instance;
        private UIWeaponPage weaponPageUI;

        private static GlobalWeaponsSave save;
        private static List<BaseWeaponUpgradeStage> keyUpgradeStages = new List<BaseWeaponUpgradeStage>();

        private static WeaponData[] weapons;
        private static Dictionary<WeaponType, int> weaponsLink;

        public static int BasePower { get; private set; }
        public static int SelectedWeaponIndex
        {
            get { return save.selectedWeaponIndex; }
            private set { save.selectedWeaponIndex = value; }
        }

        public static event SimpleCallback OnNewWeaponSelected;
        public static event SimpleCallback OnWeaponUpgraded;
        public static event SimpleCallback OnWeaponCardsAmountChanged;
        public static event WeaponDelagate OnWeaponUnlocked;

        public void Initialise()
        {
            instance = this;

            save = SaveController.GetSaveObject<GlobalWeaponsSave>("weapon_save");

            Drop.RegisterDropItem(new CustomDropItem(DropableItemType.WeaponCard, cardPrefab));

            weaponsLink = new Dictionary<WeaponType, int>();
            weapons = database.Weapons;

            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].Initialise();
                weaponsLink.Add(weapons[i].Type, i);

                BaseWeaponUpgrade baseUpgrade = UpgradesController.GetUpgrade<BaseWeaponUpgrade>(weapons[i].UpgradeType);

                BaseWeaponUpgradeStage currentStage;

                for (int j = 0; j < baseUpgrade.UpgradesCount; j++)
                {
                    currentStage = baseUpgrade.Upgrades[j] as BaseWeaponUpgradeStage;

                    if (currentStage.KeyUpgradeNumber != -1)
                    {
                        keyUpgradeStages.Add(currentStage);
                    }

                    if (currentStage.KeyUpgradeNumber == 0)
                    {
                        BasePower = currentStage.Power;
                    }
                }
            }

            weaponPageUI = UIController.GetPage<UIWeaponPage>();
            weaponPageUI.Initiaise(this);

            keyUpgradeStages.OrderBy(s => s.KeyUpgradeNumber);

            CheckWeaponUpdateState();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("[Weapon Controller] Shortcut W pressed: changing weapon");

                SelectedWeaponIndex++;
                if (SelectedWeaponIndex >= database.Weapons.Length)
                {
                    SelectedWeaponIndex = 0;
                }

                OnWeaponSelected(SelectedWeaponIndex);
            }
#endif
        }

        public static int GetCeilingKeyPower(int currentKeyUpgrade)
        {
            for (int i = keyUpgradeStages.Count - 1; i >= 0; i--)
            {
                if (keyUpgradeStages[i].KeyUpgradeNumber <= currentKeyUpgrade)
                {
                    return keyUpgradeStages[i].Power;
                }
            }

            return keyUpgradeStages[0].Power;
        }

        public void CheckWeaponUpdateState()
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                BaseUpgrade upgrade = UpgradesController.GetUpgrade<BaseUpgrade>(weapons[i].UpgradeType);

                if (upgrade.UpgradeLevel == 0 && weapons[i].CardsAmount >= upgrade.NextStage.Price)
                {
                    upgrade.UpgradeStage();

                    OnWeaponUnlocked?.Invoke(weapons[i]);
                }
            }
        }

        public static void SelectWeapon(WeaponType weaponType)
        {
            int weaponIndex = 0;
            for (int i = 0; i < instance.database.Weapons.Length; i++)
            {
                if (instance.database.Weapons[i].Type == weaponType)
                {
                    weaponIndex = i;

                    break;
                }
            }

            instance.OnWeaponSelected(weaponIndex);
        }

        public static bool IsTutorialWeaponUpgraded()
        {
            BaseUpgrade upg = UpgradesController.GetUpgrade<BaseUpgrade>(UpgradeType.Minigun);

            return upg.UpgradeLevel >= 2;
        }

        public void OnWeaponSelected(int weaponIndex)
        {
            SelectedWeaponIndex = weaponIndex;

            CharacterBehaviour.GetBehaviour().SetGun(GetCurrentWeapon(), true);
            CharacterBehaviour.GetBehaviour().Graphics.Grunt();

            OnNewWeaponSelected?.Invoke();
        }

        public static void AddCard(WeaponType weaponType, int amount)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i].Type == weaponType)
                {
                    weapons[i].Save.CardsAmount += amount;

                    break;
                }
            }

            OnWeaponCardsAmountChanged?.Invoke();
        }

        public static void AddCards(List<WeaponType> cards)
        {
            if (cards.IsNullOrEmpty())
                return;

            for (int i = 0; i < cards.Count; i++)
            {
                WeaponData weapon = weapons[weaponsLink[cards[i]]];
                weapon.Save.CardsAmount += 1;
            }

            OnWeaponCardsAmountChanged?.Invoke();
        }

        public static WeaponData GetCurrentWeapon()
        {
            return instance.database.GetWeaponByIndex(save.selectedWeaponIndex);
        }

        public static WeaponData GetWeaponData(WeaponType weaponType)
        {
            return instance.database.GetWeapon(weaponType);
        }

        public static RarityData GetRarityData(Rarity rarity)
        {
            return instance.database.GetRarityData(rarity);
        }

        public void WeaponUpgraded(WeaponData weaponData)
        {
            AudioController.PlaySound(AudioController.Sounds.upgrade);

            CharacterBehaviour characterBehaviour = CharacterBehaviour.GetBehaviour();
            characterBehaviour.SetGun(GetCurrentWeapon(), true, true, true);

            OnWeaponUpgraded?.Invoke();
        }

        public static void UnlockAllWeaponsDev()
        {
            for (int i = 0; i < instance.database.Weapons.Length; i++)
            {
                BaseUpgrade upgrade = UpgradesController.GetUpgrade<BaseUpgrade>(instance.database.Weapons[i].UpgradeType);

                if (upgrade.UpgradeLevel == 0)
                {
                    upgrade.UpgradeStage();
                }
            }
        }

        public static BaseWeaponUpgrade GetCurrentWeaponUpgrade()
        {
            return UpgradesController.GetUpgrade<BaseWeaponUpgrade>(GetCurrentWeapon().UpgradeType);
        }

        public static BaseWeaponUpgrade GetWeaponUpgrade(WeaponType type)
        {
            return UpgradesController.GetUpgrade<BaseWeaponUpgrade>(GetWeaponData(type).UpgradeType);
        }

        public static bool IsWeaponUnlocked(WeaponType type)
        {
            for (int i = 0; i < instance.database.Weapons.Length; i++)
            {
                if (instance.database.Weapons[i].Type == type)
                {
                    BaseUpgrade upgrade = UpgradesController.GetUpgrade<BaseUpgrade>(instance.database.Weapons[i].UpgradeType);

                    return upgrade.UpgradeLevel > 0;
                }
            }

            return false;
        }

        [System.Serializable]
        public class GlobalWeaponsSave : ISaveObject
        {
            public int selectedWeaponIndex;

            public GlobalWeaponsSave()
            {
                selectedWeaponIndex = 0;
            }

            public void Flush()
            {

            }
        }

        public delegate void WeaponDelagate(WeaponData weapon);
    }
}