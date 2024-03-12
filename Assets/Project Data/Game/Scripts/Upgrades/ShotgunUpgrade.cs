using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Shotgun Upgrade", menuName = "Content/Upgrades/Shotgun Upgrade")]
    public class ShotgunUpgrade : BaseWeaponUpgrade
    {
        public override void Initialise()
        {

        }

        [System.Serializable]
        public class ShotgunUpgradeStage : BaseWeaponUpgradeStage
        {
            
        }
    }
}