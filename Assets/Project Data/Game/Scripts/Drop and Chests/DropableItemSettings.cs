using UnityEngine;

namespace Watermelon.SquadShooter
{
    [CreateAssetMenu(fileName = "Dropable Item Settings", menuName = "Content/Dropable Item Settings")]
    public class DropableItemSettings : ScriptableObject
    {
        [SerializeField] CustomDropItem[] customDropItems;
        public CustomDropItem[] CustomDropItems => customDropItems;

        [SerializeField] DropAnimation[] dropAnimations;
        public DropAnimation[] DropAnimations => dropAnimations;
    }
}
