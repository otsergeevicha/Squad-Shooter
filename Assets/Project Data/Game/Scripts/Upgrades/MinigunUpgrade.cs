using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Minigun Upgrade", menuName = "Content/Upgrades/Minigun Upgrade")]
    public class MinigunUpgrade : BaseWeaponUpgrade
    {
        public override void Initialise()
        {

        }

        [System.Serializable]
        public class MinigunUpgradeStage : BaseWeaponUpgradeStage
        {
            
        }
    }
}