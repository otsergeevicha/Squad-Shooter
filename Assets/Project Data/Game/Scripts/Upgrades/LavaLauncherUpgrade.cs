using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Lava Launcher Upgrade", menuName = "Content/Upgrades/Lava Launcher Upgrade")]
    public class LavaLauncherUpgrade : BaseWeaponUpgrade
    {
        public override void Initialise()
        {

        }

        [System.Serializable]
        public class LavaLauncherUpgradeStage : BaseWeaponUpgradeStage
        {
            [SerializeField] DuoFloat bulletHeight;
            public DuoFloat BulletHeight => bulletHeight;
        }
    }
}