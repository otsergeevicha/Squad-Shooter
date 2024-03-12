using UnityEngine;
using Watermelon;

namespace Watermelon.SquadShooter
{
    [System.Serializable]
    public class WeaponData
    {
        [SerializeField] string name;
        public string Name => name;

        [SerializeField] WeaponType type;
        public WeaponType Type => type;

        [SerializeField] UpgradeType upgradeType;
        public UpgradeType UpgradeType => upgradeType;

        [SerializeField] Rarity rarity;
        public Rarity Rarity => rarity;

        [SerializeField] Sprite icon;
        public Sprite Icon => icon;

        public RarityData RarityData => WeaponsController.GetRarityData(rarity);

        private WeaponSave save;
        public WeaponSave Save => save;

        public int CardsAmount => save.CardsAmount;

        public void Initialise()
        {
            save = SaveController.GetSaveObject<WeaponSave>(string.Format("Weapon_{0}", type));
        }
    }
}