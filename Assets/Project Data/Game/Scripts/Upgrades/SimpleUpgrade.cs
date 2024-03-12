using UnityEngine;
using Watermelon.Upgrades;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Simple Upgrade", menuName = "Content/Upgrades/Simple Upgrade")]
    public class SimpleUpgrade : Upgrade<SimpleUpgrade.SimpleUpgradeStage>
    {
        public override void Initialise()
        {

        }

        [System.Serializable]
        public class SimpleUpgradeStage: BaseUpgradeStage
        {

        }
    }
}