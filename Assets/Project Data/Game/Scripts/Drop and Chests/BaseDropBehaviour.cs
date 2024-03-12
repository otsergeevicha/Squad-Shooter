using UnityEngine;

namespace Watermelon.SquadShooter
{
    // use ItemDropBehaviour if you DON'T NEED any special behaviour for the item
    // inherit this class to implement a unique behaviour 
    // important make sure to add Check If Abowe Water after item is droped
    public abstract class BaseDropBehaviour : MonoBehaviour, IDropableItem
    {
        public bool IsRewarded { get; set; } = false;

        protected bool isPicked = false;
        public bool IsPicked => isPicked;

        public GameObject Object => gameObject;

        protected DropData dropData;
        public DropData DropData => dropData;

        public int DropAmount => dropData.amount;
        public DropableItemType DropType => dropData.dropType;

        protected float availableToPickDelay;
        protected float autoPickDelay;

        public abstract void Initialise(DropData dropData, float availableToPickDelay = -1f, float autoPickDelay = -1f, bool ignoreCollector = false);
        public abstract void Pick(bool moveToPlayer = true);
        public abstract void Throw(Vector3 position, AnimationCurve movemenHorizontalCurve, AnimationCurve movementVerticalCurve, float time);
        public abstract void Drop();

        public virtual bool IsPickable(CharacterBehaviour characterBehaviour)
        {
            return true;
        }
    }
}