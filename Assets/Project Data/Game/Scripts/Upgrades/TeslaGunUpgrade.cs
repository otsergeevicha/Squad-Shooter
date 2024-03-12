using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Tesla Gun Upgrade", menuName = "Content/Upgrades/Tesla Gun Upgrade")]
    public class TeslaGunUpgrade : BaseWeaponUpgrade
    {
        public override void Initialise()
        {

        }

        [System.Serializable]
        public class TeslaGunUpgradeStage : BaseWeaponUpgradeStage
        {

        }
    }
}